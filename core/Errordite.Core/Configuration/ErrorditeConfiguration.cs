
using System.Collections.Generic;
using System.Reflection;
using Errordite.Core.Domain.Master;
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
        public string ReceiveWebEndpoints { get; set; }
        public bool RenderMinifiedContent { get; set; }
        public bool ServiceBusEnabled { get; set; }
        public string ReceiveQueueAddress { private get; set; }
        public string NotificationsQueueAddress { private get; set; }
        public string EventsQueueAddress { private get; set; }
        public string AdministratorsEmail { get; set; }
        public int MaxPageSize { get; set; }
        public int IssueErrorLimit { get; set; }
        public int IssueCacheId { get; set; }
        public int TrialLengthInDays { get; set; }
        public int QueueVisibilityTimeoutSeconds { get; set; }
        public double IssueCacheTimeoutMinutes { get; set; }
        public List<string> ErrorPropertiesForFiltering { get; set; }
        public List<RateLimiterRule> RateLimiterRules { get; set; }
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public string DeveloperQueueSuffix { get; set; }

        public string GetReceiveQueueAddress(string organisationId, RavenInstance instance = null)
        {
            if (instance == null || instance.Id == RavenInstance.Master().Id)
                return "{0}1{2}".FormatWith(ReceiveQueueAddress, DeveloperQueueSuffix);

            return "{0}{1}{2}".FormatWith(instance.ReceiveQueueAddress, instance.FriendlyId, DeveloperQueueSuffix);
        }

        public string GetEventsQueueAddress(RavenInstance instance = null)
        {
            if (instance == null)
                return "{0}1{2}".FormatWith(EventsQueueAddress, DeveloperQueueSuffix);

            return "{0}{1}{2}".FormatWith(instance.EventsQueueAddress, instance.FriendlyId, DeveloperQueueSuffix);
        }

        public string GetNotificationsQueueAddress(RavenInstance instance = null)
        {
            if (instance == null)
                return "{0}1{2}".FormatWith(NotificationsQueueAddress, DeveloperQueueSuffix);

            return "{0}{1}{2}".FormatWith(instance.NotificationsQueueAddress, instance.FriendlyId, DeveloperQueueSuffix);
        }
    }
}
