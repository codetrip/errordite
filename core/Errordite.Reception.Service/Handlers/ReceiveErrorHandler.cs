using Errordite.Core.Messages;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Reception.Commands;
using Errordite.Core.ServiceBus;
using CodeTrip.Core.Extensions;

namespace Errordite.Reception.Service.Handlers
{
    public class ReceiveErrorHandler : MessageHandlerSessionBase<ReceiveErrorMessage>
    {
        private readonly IReceiveErrorCommand _receiveErrorCommand;
        private readonly IGetOrganisationQuery _getOrganisationQuery;

        public ReceiveErrorHandler(IReceiveErrorCommand receiveErrorCommand, IGetOrganisationQuery getOrganisationQuery)
        {
            _receiveErrorCommand = receiveErrorCommand;
            _getOrganisationQuery = getOrganisationQuery;
        }

        protected override void HandleMessage(ReceiveErrorMessage message)
        {  
            if(message.ExistingIssueId.IsNotNullOrEmpty())
            {
                message.Error.Id = null;
                message.Error.IssueId = null;
            }

            if (message.OrganisationId != null)
            {
                var org =
                    _getOrganisationQuery.Invoke(new GetOrganisationRequest() {OrganisationId = message.OrganisationId})
                        .Organisation;
                if (org != null)
                {
                    Session.SetOrg(org);
                }
                else
                {
                    Trace("Organisation {0} not found", message.OrganisationId);
                    return;
                }
            }

            _receiveErrorCommand.Invoke(new ReceiveErrorRequest
            {
                Error = message.Error,
                ApplicationId = message.ApplicationId,
                OrganisationId = message.OrganisationId,
                Token = message.Token,
                ExistingIssueId = message.ExistingIssueId
            });
        }
    }
}
