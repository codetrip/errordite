using System.Collections.Generic;
using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{

    [ProtoContract]
    public class Notification
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public NotificationType Type { get; set; }
        [ProtoMember(3)]
        public IList<string> Groups { get; set; }
    }

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
