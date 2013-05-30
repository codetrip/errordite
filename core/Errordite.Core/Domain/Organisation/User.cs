using System;
using System.Collections.Generic;
using Errordite.Core.Authorisation;
using Errordite.Core.Extensions;
using Errordite.Core.Identity;
using ProtoBuf;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract, Serializable]
    public class User : IOrganisationEntity
    {
        public User()
        {
            GroupIds = new List<string>();
        }

        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public List<string> GroupIds { get; set; }
        [ProtoMember(3)]
        public string FirstName { get; set; }
        [ProtoMember(4)]
        public string LastName { get; set; }
        [ProtoMember(5)]
        public string Email { get; set; }
        [ProtoMember(6)]
        public UserRole Role { get; set; }
        [ProtoMember(7)]
		public UserStatus Status { get; set; }
		[ProtoMember(8)]
		public string OrganisationId { get; set; }

        [JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        [JsonIgnore, ProtoMember(8)]
		public Organisation ActiveOrganisation { get; set; }
		[JsonIgnore, ProtoMember(9)]
		public List<Organisation> Organisations { get; set; }
        [JsonIgnore, ProtoMember(10)]
        public List<Group> Groups { get; set; }
        [JsonIgnore]
        public string FullName
        {
            get { return "{0} {1}".FormatWith(FirstName, LastName); }
        }

        public SpecialUser? SpecialUser { get; set; }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "users/{0}".FormatWith(friendlyId);
        }

        public bool IsAdministrator()
        {
            return Role == UserRole.Administrator || Role == UserRole.SuperUser;
        }

        private static readonly User _systemUser = new User();
        public static User System()
        {
            return _systemUser;
        }
    }

    [ProtoContract]
    public enum UserStatus
    {
        [ProtoMember(1)]
        Active,
        [ProtoMember(2)]
        Inactive
    }

    public enum SpecialUser
    {
        AppHarbor
    }
}
