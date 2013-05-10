
namespace Errordite.Core.Notifications.Sending
{
    public class Message
    {
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set;}
        public string Body { get; set; }
        public string ReplyTo { get; set; }
    }
}