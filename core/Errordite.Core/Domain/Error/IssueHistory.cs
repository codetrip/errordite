
using System;
using ProtoBuf;

namespace Errordite.Core.Domain.Error
{
    [ProtoContract]
    public class IssueHistory
    {
        [ProtoMember(1)]
        public string Message { get; set; }
        [ProtoMember(2)]
        public string UserId { get; set; }
        [ProtoMember(3)]
        public DateTime DateAddedUtc { get; set; }
        [ProtoMember(4)]
        public bool SystemMessage { get; set; }
        [ProtoMember(5)]
        public string Reference { get; set; }
    }
}
