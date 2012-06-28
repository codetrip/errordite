
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace CodeTrip.Core.IoC
{
    public abstract class MasterInstallerBase : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (var installer in Installers)
                container.Install(installer);
        }

        protected abstract IEnumerable<IWindsorInstaller> Installers { get; }
    }
}