
using Errordite.Core.Extensions;
using Errordite.Core.Configuration;
using ProtoBuf;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Domain.Central
{
    [ProtoContract]
    public class RavenInstance
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        /// <summary>
        /// Endpoint for this Raven server
        /// </summary>
        [ProtoMember(2)]
        public string RavenUrl { get; set; }
        /// <summary>
        /// Indicates this is the active server, all new organisations should be added to this server
        /// </summary>
        [ProtoMember(3)]
        public bool Active { get; set; }
        /// <summary>
        /// Is the instance where the Master Errordite database lives?
        /// </summary>
        [ProtoMember(4)]
        public bool IsMaster { get; set; }
        /// <summary>
        /// Endpoint for this Raven server
        /// </summary>
        [ProtoMember(5)]
        public string ReceiveHttpEndpoint { get; set; }
        /// <summary>
        /// Receive service queue address for this instance
        /// </summary>
        [ProtoMember(6)]
        public string ReceiveQueueAddress { get; set; }
        /// <summary>
        /// Events service queue address for this instance
        /// </summary>
        [ProtoMember(7)]
        public string EventsQueueAddress { get; set; }
        /// <summary>
        /// Notifications service queue address for this instance
        /// </summary>
        [ProtoMember(8)]
        public string NotificationsQueueAddress { get; set; }

        private static readonly RavenInstance _master = new RavenInstance
        {
            Active = true,
            IsMaster = true,
            Id = "RavenInstances/1",
            ReceiveHttpEndpoint = ErrorditeConfiguration.Current.ReceiveHttpEndpoint,
            ReceiveQueueAddress = ErrorditeConfiguration.Current.GetNotificationsQueueAddress(),
            EventsQueueAddress = ErrorditeConfiguration.Current.GetEventsQueueAddress(),
            NotificationsQueueAddress = ErrorditeConfiguration.Current.GetNotificationsQueueAddress()
        };

        public static RavenInstance Master()
        {
            return _master;
        }

        public string ServiceHttpEndpoint
        {
            get { return "http://services{0}.errordite.com".FormatWith(Id.GetFriendlyId() == "1" ? string.Empty : Id.GetFriendlyId()); }
        }

        [JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "RavenInstances/{0}".FormatWith(friendlyId);
        }
    }
}
