using System;

namespace Errordite.Core.Messaging
{   
    public class SyncIssueErrorCountsMessage : MessageBase
    {
        public string IssueId { get; set; }
        public DateTime TriggerEventUtc { get; set; }
    }
}
