using Errordite.Core.ServiceBus;

namespace Errordite.Core.Messages
{
    public class CheckIssueMessage : ErrorditeNServiceBusMessageBase
    {
        public string IssueId { get; set; }
    }
}