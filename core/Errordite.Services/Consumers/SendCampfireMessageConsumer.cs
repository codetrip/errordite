using Errordite.Core;
using Errordite.Core.Messaging;
using Errordite.Core.Notifications.Commands;

namespace Errordite.Services.Consumers
{
    public class SendCampfireMessageConsumer : ComponentBase, IErrorditeConsumer<SendCampfireMessage>
    {
        private readonly ISendCampfireMessageCommand _sendCampfireCommand;

        public SendCampfireMessageConsumer(ISendCampfireMessageCommand sendCampfireCommand)
        {
            _sendCampfireCommand = sendCampfireCommand;
        }

        public void Consume(SendCampfireMessage message)
        {
            _sendCampfireCommand.Invoke(new SendCampfireMessageRequest
            {
                Message = message.Message,
				CampfireDetails = message.CampfireDetails,
				RoomId = message.RoomId
            });
        }
    }
}