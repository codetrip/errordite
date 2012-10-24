using Castle.MicroKernel.SubSystems.Configuration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CodeTrip.Core.Auditing;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.Caching.Invalidation;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Web;

namespace CodeTrip.Core.IoC
{
    public class CoreInstaller : WindsorInstallerBase
    {
        private readonly string _loggerName;

        public CoreInstaller(string loggerName)
        {
            _loggerName = loggerName;
        }

        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

            container.Register(Component.For<IEncryptor>().ImplementedBy<RijndaelSymmetricEncryptor>().LifeStyle.Singleton);
            container.Register(Component.For<ICookieManager>().ImplementedBy<CookieManager>().LifeStyle.PerWebRequest);
            container.Register(Component.For<IPagingViewModelGenerator>().ImplementedBy<PagingViewModelGenerator>().LifeStyle.PerWebRequest);

            container.Register(
                Component.For<ICacheInterceptor>()
                    .LifeStyle.Transient
                    .ImplementedBy(typeof (CacheInterceptor))
                    .Named(CacheInterceptor.IoCName),
                Component.For<ICacheInvalidator>()
                    .LifeStyle.Transient
                    .ImplementedBy(typeof(CacheInvalidator)),
                Component.For<ICacheInvalidationInterceptor>()
                    .LifeStyle.Transient
                    .ImplementedBy(typeof(CacheInvalidationInterceptor))
                    .Named(CacheInvalidationInterceptor.IoCName),
                Component.For<IComponentAuditorFactory>()
                    .ImplementedBy(typeof(ComponentAuditorFactory))
                    .LifeStyle.Transient,
                Component.For<IComponentAuditor>()
                    .LifeStyle.Transient
                    .UsingFactoryMethod(
                        kernel =>
                        kernel.Resolve<IComponentAuditorFactory>().Create(_loggerName)));
        }
    }
}