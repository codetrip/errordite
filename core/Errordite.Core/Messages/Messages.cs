
using System;
using Errordite.Core.Domain.Error;

namespace Errordite.Core.Messages
{
    public class MessageBase
    {
        public Guid Id { get; set; }
        public DateTime GeneratedOnUtc { get; set; }
        public string OrganisationId { get; set; }

        public MessageBase()
        {
            Id = Guid.NewGuid();
            GeneratedOnUtc = DateTime.UtcNow;
        }
    }

    public class ErrorReceivedMessage : MessageBase
    {
        public Error Error { get; set; }
        public string ApplicationId { get; set; }
        public string Token { get; set; }
        public string ExistingIssueId { get; set; }
    }

    public class SendMessageToHipChatRoom : MessageBase
    {
        public string Message { get; set; }
        public int HipChatRoomId { get; set; }
        public string HipChatAuthToken { get; set; }
    }
}
