using System;
using NServiceBus;

namespace CodeTrip.Core.ServiceBus
{
    public class NServiceBusMessageBase : IMessage
    {
        public Guid Id { get; set; }
        public DateTime GeneratedOnUtc { get; set; }

        public NServiceBusMessageBase()
        {
            Id = Guid.NewGuid();
            GeneratedOnUtc = DateTime.UtcNow;
        }
    }
}
