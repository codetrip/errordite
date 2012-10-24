using CodeTrip.Core.Extensions;
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
        [ProtoMember(3)]
        Investigating,
        [ProtoMember(4)]
        [FriendlyName("Awaiting Deployment")]
        AwaitingDeployment,
        [ProtoMember(5)]
        Solved,
        [ProtoMember(6)]
        Ignorable
    }
}