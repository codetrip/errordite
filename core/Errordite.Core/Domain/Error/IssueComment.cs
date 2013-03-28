using System;
using ProtoBuf;

namespace Errordite.Core.Domain.Error
{
    [ProtoContract]
    public class IssueComment
    {
        [ProtoMember(1)]
        public string UserId { get; set; }

        [ProtoMember(2)]
        public string Comment { get; set; }

        [ProtoMember(3)]
        public DateTimeOffset DateAdded { get; set; }
    }
}
    
