
using CodeTrip.Core.Extensions;
using Errordite.Core.Domain.Central;

namespace Errordite.Core.Configuration
{
    public class ServiceConfiguration
    {
        public string PortNumber { get; set; }
        public string ServiceName { get; set; }
        public string QueueName { get; set; }

        public string ErrorQueueName
        {
            get { return "{0}.error".FormatWith(QueueName); }
        }

        public string FullyQualifiedQueueName
        {
            get
            {
                return @".\private$\{0}".FormatWith(QueueName);
            }
        }

        public string FullyQualifiedErrorQueueName
        {
            get
            {
                return @".\private$\{0}".FormatWith(ErrorQueueName);
            }
        }

        public string ServiceEndpoint(RavenInstance instance)
        {
            return "services{0}.errordite.com".FormatWith(instance.Id.GetFriendlyId() == "1" ? string.Empty : instance.Id.GetFriendlyId());
        }
    }
}
