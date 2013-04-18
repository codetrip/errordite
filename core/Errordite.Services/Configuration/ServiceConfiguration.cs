using System;
using CodeTrip.Core.Extensions;

namespace Errordite.Services.Configuration
{
    public enum Service
    {
        Reception,
        Notifications,
        Events
    }

    public class ServiceConfiguration
    {
        public Service Service { get; set; }
        public int PortNumber { get; set; }
        public string QueueAddress { get; set; }
        public string MachineName { get; set; }
        public string ServiceName { get; set; }
        public string InstanceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceDiscription { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public int Threads { get; set; }
        public int RetryLimit { get; set; }
        public int MaxNumberOfMessages { get; set; }
        public int MaxOrganisationsPerMessageprocesor { get; set; }

        public string FullServiceName
        {
            get { return "{0}${1}".FormatWith(ServiceName, InstanceName); }
        }

        public string ResolvedMachineName
        {
            get { return MachineName.IsIn("localhost", ".") ? Environment.MachineName : MachineName; }
        }
    }
}
