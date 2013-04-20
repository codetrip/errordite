using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.Caching;
using Errordite.Core.IoC;
using Errordite.Core.Redis;
using Errordite.Test.Automation.Drivers;
using Errordite.Test.Automation.Drivers.ErrorditeDriver;

namespace Errordite.Test.Automation.IoC
{
    public class AutomationInstaller : WindsorInstallerBase
    {
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

            container.Install(new PerThreadAppSessionInstaller());
            container.Install(new NullCacheInstaller());
            container.Register(Component.For<IRedisSession>().ImplementedBy<RedisSession>());
            container.Register(Component.For<IOrganisationDriver>()
                .ImplementedBy(typeof(OrganisationDriver)).LifeStyle.PerThread,
                Component.For<RavenDriver>().LifeStyle.PerThread,
                Component.For<Armoury>().LifeStyle.PerThread,
                Component.For<AutomationSession>().LifeStyle.PerThread,
                Component.For<ErrorditeDriver>().LifeStyle.PerThread,
                Component.For<ErrorditeClientDriver>().LifeStyle.PerThread);
            
        }
    }
}
