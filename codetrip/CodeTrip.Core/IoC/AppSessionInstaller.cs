using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CodeTrip.Core.RavenDb;
using CodeTrip.Core.Session;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;
using Raven.Client;
using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace CodeTrip.Core.IoC
{
    public class PerWebRequestAppSessionInstaller : AppSessionInstallerBase
    {
        protected override bool InstallWithPerWebRequestLifestyle
        {
            get { return true; }
        }
    }

    public class PerThreadAppSessionInstaller : AppSessionInstallerBase
    {
        protected override bool InstallWithPerWebRequestLifestyle
        {
            get { return false; }
        }
    }

    public abstract class AppSessionInstallerBase : IWindsorInstaller
    {
        protected abstract bool InstallWithPerWebRequestLifestyle { get; }
        
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IAppSession>().ImplementedBy<AppSession>().AssignPerWebRequestOrPerThread(InstallWithPerWebRequestLifestyle),
                Component.For<IRavenDocumentStoreFactory>().ImplementedBy<RavenDocumentStoreFactory>().LifeStyle.Singleton,
                Component.For<IDocumentStore>().UsingFactoryMethod(k => k.Resolve<IRavenDocumentStoreFactory>().Create()).LifeStyle.Singleton);

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