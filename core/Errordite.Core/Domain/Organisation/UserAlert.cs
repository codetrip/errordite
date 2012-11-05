using System;
using Errordite.Core.Authorisation;
using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public class UserAlert : IUserEntity
    {
        public UserAlert()
        {
            Replacements = new string[0];
            SentUtc = DateTime.UtcNow;
        }
        
        [ProtoMember(1)]
        public string Message { get; set; }
        [ProtoMember(2)]
        public string[] Replacements { get; set; }
        [ProtoMember(3)]
        public string Id { get; set; }
        [ProtoMember(4)]
        public DateTime SentUtc { get; set; }
        [ProtoMember(5)]
        public string UserId { get; set; }
        [ProtoMember(6)]
        public string Type { get; set; }
    }
}