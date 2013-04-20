using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.Raven;
using Errordite.Core.Session;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;
using Raven.Client;
using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Errordite.Core.IoC
{
    public class ScopedAppSessionInstaller : AppSessionInstallerBase
    {
        protected override ComponentRegistration<T> PerUnitOfWorkLifeStyleRegistration<T>(ComponentRegistration<T> registration)
        {
            //LifestyleScoped means you get the same object within the scope of the container (which you create by calling container.BeginScope())
            return registration.LifestyleScoped();
        }
    }

    public class PerWebRequestAppSessionInstaller : AppSessionInstallerBase
    {
        protected override ComponentRegistration<T> PerUnitOfWorkLifeStyleRegistration<T>(ComponentRegistration<T> registration)
        {
            return registration.LifeStyle.PerWebRequest;
        }
    }

    public class PerThreadAppSessionInstaller : AppSessionInstallerBase
    {
        protected override ComponentRegistration<T> PerUnitOfWorkLifeStyleRegistration<T>(ComponentRegistration<T> registration)
        {
            return registration.LifeStyle.PerThread;
        }
    }

    public abstract class AppSessionInstallerBase : IWindsorInstaller
    {
        protected abstract ComponentRegistration<T> PerUnitOfWorkLifeStyleRegistration<T>(
            ComponentRegistration<T> registration) where T : class;

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                PerUnitOfWorkLifeStyleRegistration(Component.For<IAppSession>().ImplementedBy<AppSession>()),
                Component.For<IShardedRavenDocumentStoreFactory>().ImplementedBy<ShardedRavenDocumentStoreFactory>().LifeStyle.Singleton);

        }
    }

    public interface IWantToKnowAboutProdProf
    {
        void TellMe(ProfiledRequestData data);
    }

    public class AddProdProfInfoListener : IDocumentStoreListener
    {
        public bool BeforeStore(string key, object entityInstance, RavenJObject metadata, RavenJObject original)
        {
            var wantToKnow = entityInstance as IWantToKnowAboutProdProf;

            if (ProfilerContext.Configuration != null && wantToKnow != null)
            {
                var data = ProfilerContext.Configuration.GetCurrentProfiledData();
                if (data != null)
                    wantToKnow.TellMe(data);
            }

            return true;
        }

        public void AfterStore(string key, object entityInstance, RavenJObject metadata)
        {

        }
    }
}