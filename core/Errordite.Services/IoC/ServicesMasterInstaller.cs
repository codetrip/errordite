using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using CodeTrip.Core.IoC;
using Errordite.Core.IoC;

namespace Errordite.Services.IoC
{
    public class ServicesMasterInstaller : MasterInstallerBase
    {
        protected override IEnumerable<IWindsorInstaller> Installers
        {
            get
            {
                return new IWindsorInstaller[]
                {
                    new CoreInstaller("Errordite.Reception"),
                    new ErrorditeCoreInstaller(),
                    new ScopedAppSessionInstaller(), 
                    new ServicesInstaller(),
                };
            }
        }
    }
}