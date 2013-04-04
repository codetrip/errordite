using System;

namespace Errordite.Core.Monitoring.Entities
{
    public class QueueStatus
    {
        public string QueueName { get; set; }
        public string Message { get; set; }
        public int TotalMessages { get; set; }
        public DateTime? EarliestMessage { get; set; }
    }
}