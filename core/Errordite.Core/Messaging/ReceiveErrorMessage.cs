using Errordite.Core.Domain.Error;

namespace Errordite.Core.Messaging
{
    public class ReceiveErrorMessage : MessageBase
    {
        public Error Error { get; set; }
        public string ApplicationId { get; set; }
        public string Token { get; set; }
        public string ExistingIssueId { get; set; } 
    }
}
