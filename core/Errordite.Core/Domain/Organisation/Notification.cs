using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public enum NotificationType
    {
        [ProtoMember(1)]
        NotifyOnNewIssueCreated,
        [ProtoMember(2)]
        NotifyOnNewInstanceOfSolvedIssue,
        [ProtoMember(3)]
        NotifySystemWarnings,
        AlwaysNotifyOnInstanceOfIssue
    }
}
