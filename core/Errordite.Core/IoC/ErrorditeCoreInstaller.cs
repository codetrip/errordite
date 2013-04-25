using Amazon.SQS;
using Amazon.SimpleEmail;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Errordite.Core.Auditing;
using Errordite.Core.Auditing.Entities;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.Caching.Invalidation;
using Errordite.Core.Encryption;
using Errordite.Core.Identity;
using Errordite.Core.Issues;
using Errordite.Core.Matching;
using Errordite.Core.Messaging;
using Errordite.Core.Notifications.Naming;
using Errordite.Core.Notifications.Parsing;
using Errordite.Core.Notifications.Rendering;
using Errordite.Core.Notifications.Sending;
using Errordite.Core.Extensions;
using Errordite.Core.Paging;
using Errordite.Core.Reception;
using Errordite.Core.Web;

namespace Errordite.Core.IoC
{
    public class ErrorditeCoreInstaller : WindsorInstallerBase
    {
        private readonly string _loggerName;

        public ErrorditeCoreInstaller(string loggerName)
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
                    .ImplementedBy(typeof(CacheInterceptor))
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

            container.Register(Component.For<IMessageSender>()
                .ImplementedBy<AmazonSQSMessageSender>()
                .LifeStyle.Transient);

            container.Register(Component.For<IAuthenticationManager>().ImplementedBy<AuthenticationManager>().LifeStyle.Transient);
            container.Register(Component.For<IAppContextFactory>()
                .ImplementedBy<AppContextFactory>()
                .Forward<IImpersonationManager>()
                .LifeStyle.Transient);

            container.Register(Component.For<IReceptionServiceIssueCache>()
                .ImplementedBy<ReceptionServiceIssueCache>()
                .LifeStyle.Transient);

            container.Register(Component.For<IExceptionRateLimiter>()
                .ImplementedBy<ExceptionRateLimiter>()
                .LifeStyle.Singleton);

            container.Register(Component.For<IDateTime>()
               .ImplementedBy<UtcDateTime>()
               .LifeStyle.Singleton);

            container.Register(
                Component.For<AppContext>().ImplementedBy<AppContext>().UsingFactoryMethod(
                    kernel => kernel.Resolve<IAppContextFactory>().Create()).LifeStyle.Transient);

            container.Register(Component.For<IAuthorisationManager>()
                .ImplementedBy<AuthorisationManager>()
                .LifeStyle.Transient);

            container.Register(Component.For<IMatchRuleFactoryFactory>()
                .ImplementedBy<MatchRuleFactoryFactory>()
                .LifeStyle.Singleton);

            container.Register(Component.For<IMatchRuleFactory>()
                .ImplementedBy<MethodAndTypeMatchRuleFactory>()
                .LifeStyle.Transient
                .Named(CoreConstants.MatchRuleFactoryIdFormat.FormatWith(new MethodAndTypeMatchRuleFactory().Id)));

            container.Register(Component.For<IMatchRuleFactory>()
                .ImplementedBy<ModuleAndTypeMatchRuleFactory>()
                .LifeStyle.Transient
                .Named(CoreConstants.MatchRuleFactoryIdFormat.FormatWith(new ModuleAndTypeMatchRuleFactory().Id)));

            container.Register(Component.For<IErrorditeCore>()
                .ImplementedBy<ErrorditeCore>()
                .LifeStyle.Transient);
            
            container.Register(Component.For<IEmailNamingMapper>().ImplementedBy<ConventionalEmailNamingMapper>().LifeStyle.Transient);
            container.Register(Component.For<IEmailSender>().ImplementedBy<SmtpEmailSender>().LifeStyle.Transient);
            container.Register(Component.For<ITemplateLocator>().ImplementedBy<TemplateLocator>().LifeStyle.Transient);
            container.Register(Component.For<IEmailRenderer>().ImplementedBy<EmailRenderer>().LifeStyle.Transient);
            container.Register(Component.For<IEmailInfoParser>().ImplementedBy<EmailInfoParser>().LifeStyle.Transient);

            container.Register(Component.For<IAmazonSQSFactory>()
                .ImplementedBy<AmazonSQSFactory>()
                .LifeStyle.Singleton);

            container.Register(Component.For<AmazonSQS>()
                .UsingFactoryMethod(kernel => kernel.Resolve<IAmazonSQSFactory>().Create())
                .LifeStyle.Singleton);

            container.Register(Component.For<IAmazonSimpleEmailFactory>()
                .ImplementedBy<AmazonSimpleEmailFactory>()
                .LifeStyle.Singleton);

            container.Register(Component.For<AmazonSimpleEmailServiceClient>()
                .UsingFactoryMethod(kernel => kernel.Resolve<IAmazonSimpleEmailFactory>().Create())
                .LifeStyle.Singleton);
        }
    }
}