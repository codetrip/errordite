using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
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

            var errors = Session.Raven.Query<ErrorDocument, Errors_Search>()
                .Where(e => e.IssueId == request.IssueId)
                .OrderByDescending(e => e.TimestampUtc)
                .Skip(_configuration.IssueErrorLimit)
                .As<Error>()
                .ToList();

            Trace("Identified {0} errors to make unlogged", errors.Count);

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
