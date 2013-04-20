
using System.Collections.Generic;
using System.Reflection;
using Errordite.Core.Extensions;
using Errordite.Core.IoC;
using Errordite.Core.Reception;

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
        public int TrialLengthInDays { get; set; }
        public double IssueCacheTimeoutMinutes { get; set; }
        public string ReceptionHttpEndpoint { get; set; }
        public List<string> ErrorPropertiesForFiltering { get; set; }
        public List<RateLimiterRule> RateLimiterRules { get; set; }

        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
    }
}
