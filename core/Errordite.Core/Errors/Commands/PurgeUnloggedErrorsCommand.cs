using CodeTrip.Core.Interfaces;
using Errordite.Core.Organisations;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Errors.Commands
{
    public class PurgeUnloggedErrorsCommand : SessionAccessBase, IPurgeUnloggedErrorsCommand
    {
        public PurgeUnloggedErrorsResponse Invoke(PurgeUnloggedErrorsRequest request)
        {
            Trace("Starting...");
            

            return new PurgeUnloggedErrorsResponse();
        }
    }

    public interface IPurgeUnloggedErrorsCommand : ICommand<PurgeUnloggedErrorsRequest, PurgeUnloggedErrorsResponse>
    { }

    public class PurgeUnloggedErrorsResponse
    { }

    public class PurgeUnloggedErrorsRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
    }
}
