
namespace Errordite.Core.Messaging
{
    public class SendHipChatMessage : MessageBase
    {
        public string Message { get; set; }
        public int HipChatRoomId { get; set; }
        public string HipChatAuthToken { get; set; }
    }
}
