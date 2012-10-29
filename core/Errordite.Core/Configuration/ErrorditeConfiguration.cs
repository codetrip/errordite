
using System.Collections.Generic;
using System.Reflection;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.IoC;

namespace Errordite.Core.Configuration
{
    public class ErrorditeConfiguration
    {
        public static ErrorditeConfiguration Current { get { return ObjectFactory.GetObject<ErrorditeConfiguration>(); } }

        public static readonly string CurrentBuildNumber = Assembly.GetExecutingAssembly().GetCurrentBuildNumber();
        public string SiteBaseUrl { get; set; }
        public string ReceptionEndpoint { get; set; }
        public bool RenderMinifiedContent { get; set; }
        public bool ServiceBusEnabled { get; set; }
        public string ReceptionQueueName { get; set; }
        public string NotificationsQueueName { get; set; }
        public string EventsQueueName { get; set; }
        public string AdministratorsEmail { get; set; }
        public int MaxPageSize { get; set; }
        public int IssueErrorLimit { get; set; }
        public int IssueCacheId { get; set; }
        public double IssueCacheTimeoutMinutes { get; set; }
        public string ReceptionHttpEndpoint { get; set; }
        public List<string> ErrorPropertiesForFiltering { get; set; }
        //public List<RateLimiterRule> RateLimiterRules { get; set; }
    }
}
