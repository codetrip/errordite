using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using CodeTrip.Core.IoC;
using Errordite.Core.IoC;

namespace Errordite.Notifications.Service.IoC
{
    public class NotificationsServiceInstaller : MasterInstallerBase
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
                };
            }
        }
    }
}