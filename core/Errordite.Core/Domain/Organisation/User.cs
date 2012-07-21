using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using Errordite.Core.Authorisation;
using ProtoBuf;

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
        public string OrganisationId { get; set; }
        [ProtoMember(3)]
        public List<string> GroupIds { get; set; }
        [ProtoMember(4)]
        public string FirstName { get; set; }
        [ProtoMember(5)]
        public string LastName { get; set; }
        [ProtoMember(6)]
        public string Email { get; set; }
        [ProtoMember(7)]
        public string Password { get; set; }
        [ProtoMember(8)]
        public Guid PasswordToken { get; set; }
        [ProtoMember(9)]
        public UserRole Role { get; set; }
        [ProtoMember(10)]
        public UserStatus Status { get; set; }
        [ProtoMember(13)]
        public string TimezoneId { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore, ProtoMember(11)]
        public Organisation Organisation { get; set; }
        [Raven.Imports.Newtonsoft.Json.JsonIgnore, ProtoMember(12)]
        public List<Group> Groups { get; set; }
        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string FullName
        {
            get { return "{0} {1}".FormatWith(FirstName, LastName); }
        }

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

        public string EffectiveTimezoneId()
        {
            return TimezoneId ?? Organisation.TimezoneId ?? "UTC";
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
}
