using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Errordite.Core.IoC;
using Errordite.Core.IoC;

namespace Errordite.Notifications.Service.IoC
{
    public class NotificationsMasterInstaller : MasterInstallerBase
    {
        protected override IEnumerable<IWindsorInstaller> Installers
        {
            get
            {
                return new IWindsorInstaller[]
                {
                    new CoreInstaller("Errordite.Notifications"),
                    new ErrorditeCoreInstaller(),
                    new NotificationsServiceNServiceBusInstaller(),
                    new PerThreadAppSessionInstaller(),
                    new NotificationsServiceInstaller(), 
                };
            }
        }
    }
}