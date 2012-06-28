using CodeTrip.Core.ServiceBus;
using Errordite.Core.Notifications.EmailInfo;

namespace Errordite.Core.IoC
{
    public class ErrorditeNServiceBusClientInstaller : NServiceBusClientInstaller
    {
        public ErrorditeNServiceBusClientInstaller()
            : base(new[]
                {
                    typeof (EmailInfoBase).Assembly
                })
        { }
    }
}