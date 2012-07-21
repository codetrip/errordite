using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public enum NotificationType
    {
        [ProtoMember(1)]
        NotifyOnNewClassCreated,
        [ProtoMember(2)]
        NotifyOnNewInstanceOfSolvedClass,
        [ProtoMember(3)]
        NotifySystemWarnings
    }
}
