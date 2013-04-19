using System;
using Errordite.Core.Extensions;

namespace Errordite.Services.Configuration
{
    public enum ServiceInstance
    {
        Reception,
        Notifications,
        Events
    }

    public class ServiceConfiguration
    {
        public ServiceInstance Instance { get; set; }
        public int PortNumber { get; set; }
        public string QueueAddress { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceDiscription { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public int QueueProcessingThreads { get; set; }
        public int RetryLimit { get; set; }
        public int RetryDelayMilliseconds { get; set; }
        public int MaxNumberOfMessages { get; set; }
        public int MaxOrganisationsPerMessageProcesor { get; set; }

        public string FullServiceName
        {
            get { return "{0}${1}".FormatWith(ServiceName, Instance.ToString()); }
        }
    }
}
