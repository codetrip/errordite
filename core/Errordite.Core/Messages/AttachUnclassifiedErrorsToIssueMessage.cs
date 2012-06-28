
using Errordite.Core.ServiceBus;

namespace Errordite.Core.Messages
{
    public class AttachUnclassifiedErrorsToIssueMessage : ErrorditeNServiceBusMessageBase
    {
        public string IssueId { get; set; }
    }
}
