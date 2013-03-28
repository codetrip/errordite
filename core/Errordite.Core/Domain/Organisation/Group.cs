
using CodeTrip.Core.Extensions;
using Errordite.Core.Authorisation;
using ProtoBuf;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public class Group : IOrganisationEntity
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string OrganisationId { get; set; }
        [ProtoMember(3)]
        public string Name { get; set; }

        [JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }
        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "groups/{0}".FormatWith(friendlyId);
        }
    }
}
