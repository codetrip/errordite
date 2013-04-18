using System;
using CodeTrip.Core.Extensions;

namespace Errordite.Services
{
    public class ServiceConfiguration
    {
        public string Id { get; set; }
        public string HttpEndpoint { get; set; }
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
