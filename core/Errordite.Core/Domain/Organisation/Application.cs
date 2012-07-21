
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using Errordite.Core.Authorisation;
using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public class Application : IOrganisationEntity
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string OrganisationId { get; set; }
        [ProtoMember(3)]
        public string Token { get; set; }
        [ProtoMember(4)]
        public string Name { get; set; }
        [ProtoMember(5)]
        public string DefaultUserId { get; set; }
        [ProtoMember(6)]
        public bool IsActive { get; set; }
        [ProtoMember(7)]
        public string MatchRuleFactoryId { get; set; }
        [ProtoMember(8)]
        public List<string> NotificationGroups { get; set; }
        [ProtoMember(9)]
        public int? HipChatRoomId { get; set; }
        [ProtoMember(10)]
        public string HipChatAuthToken { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        [ProtoMember(11)]
        public string TokenSalt { get; set; }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "applications/{0}".FormatWith(friendlyId);
        }

        public bool SendNonEmailNotifications()
        {
            return HipChatAuthToken.IsNotNullOrEmpty() && HipChatRoomId > 0;
        }
    }
}
