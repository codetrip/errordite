using Errordite.Core.Messages;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.ServiceBus;

namespace Errordite.Notifications.Service.Handlers
{
    public class SendHipChatMessageHandler : MessageHandlerBase<SendHipChatMessage>
    {
        private readonly ISendHipChatMessageCommand _sendHipChatCommand;

        public SendHipChatMessageHandler(ISendHipChatMessageCommand sendHipChatCommand)
        {
            _sendHipChatCommand = sendHipChatCommand;
        }

        protected override void HandleMessage(SendHipChatMessage message)
        {
            _sendHipChatCommand.Invoke(new SendHipChatMessageRequest
            {
                HipChatRoomId = message.HipChatRoomId,
                HipChatAuthToken = message.HipChatAuthToken,
                Message = message.Message
            });
        }
    }
}
