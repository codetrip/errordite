using ProtoBuf;

namespace Errordite.Core.Domain.Error
{
    [ProtoContract]
    public enum IssueStatus
    {
        [ProtoMember(1)]
        Unacknowledged,
        [ProtoMember(2)]
        Acknowledged,
        FixReady,
        [ProtoMember(3)]
        Solved,
        [ProtoMember(4)]
        Ignored
    }
}