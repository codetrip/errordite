using Errordite.Core.ServiceBus;

namespace Errordite.Core.Messages
{
    public class SendHipChatMessage : ErrorditeNServiceBusMessageBase
    {
        public string Message { get; set; }
        public int HipChatRoomId { get; set; }
        public string HipChatAuthToken { get; set; }
    }
}
