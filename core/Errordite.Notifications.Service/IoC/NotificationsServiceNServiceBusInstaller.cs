using CodeTrip.Core.ServiceBus;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Notifications.Service.Handlers;

namespace Errordite.Notifications.Service.IoC
{
    public class NotificationsServiceNServiceBusInstaller : NServiceBusServerInstaller
    {
        public NotificationsServiceNServiceBusInstaller()
            : base(new[]
                {
                    typeof (NServiceBusMessageBase).Assembly,
                    typeof (EmailInfoBase).Assembly,
                    typeof (SendEmailHandler).Assembly
                })
        { }
    }
}
