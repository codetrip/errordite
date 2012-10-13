using Errordite.Core.Messages;
using Errordite.Core.Reception.Commands;
using Errordite.Core.ServiceBus;
using CodeTrip.Core.Extensions;

namespace Errordite.Reception.Service.Handlers
{
    public class ReceiveErrorHandler : MessageHandlerSessionBase<ReceiveErrorMessage>
    {
        private readonly IReceiveErrorCommand _receiveErrorCommand;

        public ReceiveErrorHandler(IReceiveErrorCommand receiveErrorCommand)
        {
            _receiveErrorCommand = receiveErrorCommand;
        }

        protected override void HandleMessage(ReceiveErrorMessage message)
        {
            if(message.ExistingIssueId.IsNotNullOrEmpty())
            {
                message.Error.Id = null;
                message.Error.IssueId = null;
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
