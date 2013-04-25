
namespace Errordite.Core.Configuration
{
    public enum Service
    {
        Receive,
        Notifications,
        Events
    }

    public class ServiceConfiguration
    {
        public Service Service { get; set; }
        public int PortNumber { get; set; }
        public string QueueAddress { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceDiscription { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }

        public int ServiceProcessorCount { get; set; }
        public int ConcurrencyRetryLimit { get; set; }
        public int ConcurrencyRetryDelayMilliseconds { get; set; }
        public int MaxNumberOfMessagesPerReceive { get; set; }
    }
}
