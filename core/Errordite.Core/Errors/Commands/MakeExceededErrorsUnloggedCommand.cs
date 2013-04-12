using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Raven.Client;
using Raven.Client.Linq;

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

            //GT: aggressively caching is no good cos we end up getting etag errors when we try to delete the same error again
            //    if we want to do this we will need some sort of local memory of errors we've already deleted recently
            //    and not try to delete them again.  I guess the same problem could happen from a 304 from an index if it
            //    has not updated with the removal so maybe we need that code anyway

            //aggresively cache this query, when reprocessing errors this gets called for each error we reprocess, its unnecessary 
            //as the extra errors will be deleted next time round anyway, so aggresively cache for 1 minute
            //using (Session.Raven.Advanced.DocumentStore.AggressivelyCacheFor(TimeSpan.FromMinutes(1)))
            //{
                errors = Session.Raven.Query<ErrorDocument, Errors_Search>()
                     .Where(e => e.IssueId == request.IssueId)
                     .OrderByDescending(e => e.TimestampUtc)
                     .Skip(_configuration.IssueErrorLimit)
                     .As<Error>()
                     .ToList();
            //}

            Trace("Identified {0} errors to make unlogged", errors.Count());

            foreach (var error in errors)
            {
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
