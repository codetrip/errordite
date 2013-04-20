using Errordite.Core.Issues.Commands;
using Errordite.Core.Messages;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.ServiceBus;

namespace Errordite.Events.Service.Handlers
{
    public class SyncIssueErrorCountsHandler : MessageHandlerSessionBase<SyncIssueErrorCountsMessage>
    {
        private readonly IResetIssueErrorCountsCommand _resetIssueErrorCountsCommand;
        private readonly IGetOrganisationQuery _getOrganisationQuery;

        public SyncIssueErrorCountsHandler(IResetIssueErrorCountsCommand resetIssueErrorCountsCommand, 
            IGetOrganisationQuery getOrganisationQuery)
        {
            _resetIssueErrorCountsCommand = resetIssueErrorCountsCommand;
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

            _resetIssueErrorCountsCommand.Invoke(new ResetIssueErrorCountsRequest
            {
                IssueId = message.IssueId,
                TriggerEventUtc = message.SentAtUtc,
            });
        }
    }
}
