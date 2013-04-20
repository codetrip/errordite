using Errordite.Core;
using Errordite.Core.Messaging;
using Errordite.Core.Notifications.Commands;

namespace Errordite.Services.Consumers
{
    public class SendHipChatMessageConsumer : ComponentBase, IErrorditeConsumer<SendHipChatMessage>
    {
        private readonly ISendHipChatMessageCommand _sendHipChatCommand;

        public SendHipChatMessageConsumer(ISendHipChatMessageCommand sendHipChatCommand)
        {
            _sendHipChatCommand = sendHipChatCommand;
        }

        public void Consume(SendHipChatMessage message)
        {
            TraceObject(message);

            _sendHipChatCommand.Invoke(new SendHipChatMessageRequest
            {
                HipChatRoomId = message.HipChatRoomId,
                HipChatAuthToken = message.HipChatAuthToken,
                Message = message.Message
            });
        }
    }
}