using Castle.MicroKernel.SubSystems.Configuration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CodeTrip.Core.IoC;
using Errordite.Core.Authorisation;
using Errordite.Core.Identity;
using Errordite.Core.Issues;
using Errordite.Core.Matching;
using Errordite.Core.Notifications.Naming;
using Errordite.Core.Notifications.Parsing;
using Errordite.Core.Notifications.Rendering;
using Errordite.Core.Notifications.Sending;
using CodeTrip.Core.Extensions;
using Errordite.Core.Reception;

namespace Errordite.Core.IoC
{
    public class ErrorditeCoreInstaller : WindsorInstallerBase
    {
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);
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

            container.Register(Component.For<IFindClosestMatchingIssue>()
                .ImplementedBy<FindClosestMatchingIssue>()
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
            container.Register(Component.For<IMessageSender>().ImplementedBy<SmtpMessageSender>().LifeStyle.Transient);
            container.Register(Component.For<ITemplateLocator>().ImplementedBy<TemplateLocator>().LifeStyle.Transient);
            container.Register(Component.For<IEmailRenderer>().ImplementedBy<EmailRenderer>().LifeStyle.Transient);
            container.Register(Component.For<IEmailInfoParser>().ImplementedBy<EmailInfoParser>().LifeStyle.Transient);
        }
    }
}