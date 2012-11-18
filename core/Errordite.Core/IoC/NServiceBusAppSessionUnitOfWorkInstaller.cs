using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NServiceBus.UnitOfWork;

namespace Errordite.Core.IoC
{
    public class NServiceBusAppSessionUnitOfWorkInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IManageUnitsOfWork>().ImplementedBy<NServiceBusHandlerUnitOfWorkManager>().LifeStyle.Transient,
                               Component.For<IWindsorContainer>().Instance(container));
            //IWindsorContainer not registered by default, but required by the UoW.  If required by other things should be moved into its own / common installer
        }
    }
}