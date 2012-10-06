using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Raven.Client.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Errors.Commands
{
    public class MakeExceededErrorsUnloggedCommand : SessionAccessBase, IMakeExceededErrorsUnloggedCommand
    {
        private readonly ErrorditeConfiguration _configuration;

        public MakeExceededErrorsUnloggedCommand(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MakeExceededErrorsUnloggedResponse Invoke(MakeExceededErrorsUnloggedRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            IEnumerable<Error> errors;

            //aggresively cache this query, when reprocessing errors this gets called for each error we reprocess, its unnecessary 
            //as the extra errors will be deleted next time round anyway, so aggresively cache for 3 minutes
            using (Session.Raven.Advanced.DocumentStore.AggressivelyCacheFor(TimeSpan.FromMinutes(3)))
            {
                errors = Session.Raven.Query<ErrorDocument, Errors_Search>()
                     .Where(e => e.IssueId == request.IssueId)
                     .OrderByDescending(e => e.TimestampUtc)
                     .Skip(_configuration.IssueErrorLimit)
                     .As<Error>()
                     .ToList();
            }

            Trace("Identified {0} errors to make unlogged", errors.Count());

            foreach (var error in errors)
            {
                Store(new UnloggedError(error));
                Delete(error);
            }

            return new MakeExceededErrorsUnloggedResponse();
        }
    }

    public interface IMakeExceededErrorsUnloggedCommand : ICommand<MakeExceededErrorsUnloggedRequest, MakeExceededErrorsUnloggedResponse>
    { }

    public class MakeExceededErrorsUnloggedResponse
    { }

    public class MakeExceededErrorsUnloggedRequest
    {
        public string IssueId { get; set; }
    }
}
