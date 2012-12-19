using Errordite.Core.Issues.Commands;
using Errordite.Core.Messages;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.ServiceBus;

namespace Errordite.Events.Service.Handlers
{
    public class SyncIssueErrorCountsHandler : MessageHandlerSessionBase<SyncIssueErrorCountsMessage>
    {
        private readonly ISyncIssueErrorCountsCommand _syncIssueErrorCountsCommand;
        private readonly IGetOrganisationQuery _getOrganisationQuery;

        public SyncIssueErrorCountsHandler(ISyncIssueErrorCountsCommand syncIssueErrorCountsCommand, 
            IGetOrganisationQuery getOrganisationQuery)
        {
            _syncIssueErrorCountsCommand = syncIssueErrorCountsCommand;
            _getOrganisationQuery = getOrganisationQuery;
        }

        protected override void HandleMessage(SyncIssueErrorCountsMessage message)
        {
            if (message.OrganisationId != null)
            {
                var org = _getOrganisationQuery.Invoke(new GetOrganisationRequest
                {
                    OrganisationId = message.OrganisationId
                }).Organisation;

                if (org != null)
                {
                    Session.SetOrganisation(org);
                }
                else
                {
                    Trace("Organisation {0} not found", message.OrganisationId);
                    return;
                }
            }

            _syncIssueErrorCountsCommand.Invoke(new SyncIssueErrorCountsRequest
            {
                IssueId = message.IssueId
            });
        }
    }
}
