using Errordite.Core;
using Errordite.Core.Messaging;
using Errordite.Core.Reception.Commands;

namespace Errordite.Services.Consumers
{
    public class ReceiveErrorConsumer : ComponentBase, IErrorditeConsumer<ReceiveErrorMessage>
    {
        private readonly IReceiveErrorCommand _receiveErrorCommand;

        public ReceiveErrorConsumer(IReceiveErrorCommand receiveErrorCommand)
        {
            _receiveErrorCommand = receiveErrorCommand;
        }

        public void Consume(ReceiveErrorMessage message)
        {
            _receiveErrorCommand.Invoke(new ReceiveErrorRequest
            {
                Error = message.Error,
                ApplicationId = message.ApplicationId,
                Organisation = message.Organisation,
                Token = message.Token,
                ExistingIssueId = message.ExistingIssueId
            });
        }
    }
}