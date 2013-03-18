
using Errordite.Core.ServiceBus;

namespace Errordite.Core.Messages
{   
    public class SyncIssueErrorCountsMessage : ErrorditeNServiceBusMessageBase
    {
        public string IssueId { get; set; }
        public string OrganisationId { get; set; }

        public SyncIssueErrorCountsMessage()
        {
            DoNotAudit = true;
        }
    }
}
