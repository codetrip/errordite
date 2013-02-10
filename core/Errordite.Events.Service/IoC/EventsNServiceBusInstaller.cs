
using CodeTrip.Core.ServiceBus;
using Errordite.Core.Messages;
using Errordite.Events.Service.Handlers;

namespace Errordite.Events.Service.IoC
{
    public class EventsNServiceBusInstaller : NServiceBusServerInstaller
    {
        public EventsNServiceBusInstaller()
            : base(new[]
                {
                    typeof (ReceiveErrorMessage).Assembly,
                    typeof (SyncIssueErrorCountsHandler).Assembly
                })
        { }
    }
}
