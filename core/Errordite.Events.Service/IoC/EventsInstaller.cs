using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using CodeTrip.Core.IoC;
using Errordite.Core.IoC;

namespace Errordite.Events.Service.IoC
{
    public class EventsInstaller : MasterInstallerBase
    {

        protected override IEnumerable<IWindsorInstaller> Installers
        {
            get
            {
                return new IWindsorInstaller[]
                {
                    new CoreInstaller("Errordite.Events"),
                    new ErrorditeCoreInstaller(),
                    new EventsNServiceBusInstaller(), 
                    new PerThreadAppSessionInstaller(),
                };
            }
        }
    }
}