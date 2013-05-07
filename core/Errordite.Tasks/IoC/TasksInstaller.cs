using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.IoC;
using Errordite.Tasks.Tasks;

namespace Errordite.Tasks.IoC
{
    public class TasksInstaller : WindsorInstallerBase
    {
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

	        container.Register(Component.For<ITask>()
		        .ImplementedBy<TrialExpirationEmailSenderTask>()
		        .Named("trialexpirations")
				.LifestyleTransient());

			container.Register(Component.For<ITask>()
				.ImplementedBy<DeleteSuspendedOrganisationsTask>()
				.Named("deletesuspendedorganisations")
				.LifestyleTransient());

			container.Register(Component.For<ITask>()
				.ImplementedBy<SuspendCancelledOrganisationsTask>()
				.Named("suspendcancelledorganisations")
				.LifestyleTransient());

			container.Register(Component.For<ITask>()
				.ImplementedBy<NotifyOrganisationsWithExceededQuotasTask>()
				.Named("notifyexceededquotas")
				.LifestyleTransient());

			container.Register(Component.For<ITask>()
				.ImplementedBy<IdentifyOrganisationsWithExceededQuotasTask>()
				.Named("identifyexceededquotas")
				.LifestyleTransient());
        }
    }
}


