using System;
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
		public string ChargifyApiKey { get; set; }
		public string ChargifyUrl { get; set; }
		public string ChargifyPassword { get { return "x"; } }
        public int RavenBackupInterval { get; set; }

	    public string GetReceiveQueueAddress(string organisationId = "1")
        {
			return "{0}{1}{2}".FormatWith(ReceiveQueueAddress, organisationId.GetFriendlyId(), DeveloperQueueSuffix);
        }

        public string GetEventsQueueAddress(string ravenInstanceId = "1")
        {
            return "{0}{1}{2}".FormatWith(EventsQueueAddress, ravenInstanceId.GetFriendlyId(), DeveloperQueueSuffix);
        }

        public string GetNotificationsQueueAddress(string ravenInstanceId = "1")
        {
			return "{0}{1}{2}".FormatWith(NotificationsQueueAddress, ravenInstanceId.GetFriendlyId(), DeveloperQueueSuffix);
        }

        public string GetQueueForService(Service service, string organisationId = null, string ravenInstanceId = null)
        {
            switch (service)
            {
                case Service.Receive:
                    return GetReceiveQueueAddress(organisationId);
                case Service.Notifications:
                    return GetNotificationsQueueAddress(ravenInstanceId);
                case Service.Events:
                    return GetEventsQueueAddress(ravenInstanceId);
            }

            throw new InvalidOperationException("Invalid service name:={0}".FormatWith(service.ToString()));
        }
    }
}
