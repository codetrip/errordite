using Errordite.Core.Extensions;
using ProtoBuf;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Domain.Master
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

		[JsonIgnore]
        public string ServicesBaseUrl
        {
            get { return "http://services{0}.errordite.com".FormatWith(Id.GetFriendlyId() == "1" ? string.Empty : Id.GetFriendlyId()); }
        }

        [JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "RavenInstances/{0}".FormatWith(friendlyId);
        }

		public static RavenInstance Master()
		{
			return _master;
		}

		[JsonIgnore]
		private static readonly RavenInstance _master = new RavenInstance
		{
			Active = true,
			IsMaster = true,
			Id = "RavenInstances/1"
		};
    }
}
