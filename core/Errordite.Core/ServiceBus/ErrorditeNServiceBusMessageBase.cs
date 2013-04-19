
using System;
using Errordite.Core.ServiceBus;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.ServiceBus
{
    public class ErrorditeNServiceBusMessageBase : NServiceBusMessageBase
    {
        public User CurrentUser { get; set; }
        public bool DoNotAudit { get; set; }
        public DateTime SentAtUtc { get; set; }
    }
}
