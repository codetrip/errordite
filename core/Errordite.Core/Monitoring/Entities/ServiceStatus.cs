using System.ServiceProcess;

namespace Errordite.Core.Monitoring.Entities
{
    public class ServiceStatus
    {
        public string ServiceName { get; set; }
        public int ProcessId { get; set; }
        public ServiceControllerStatus Status { get; set; }
        public QueueStatus InputQueueStatus { get; set; }
        public QueueStatus ErrorQueueStatus { get; set; }
    }
}
