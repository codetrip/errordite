using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CodeTrip.Core.IoC;
using Errordite.Core.IoC;

namespace Errordite.Reception.Service.IoC
{
    public class ReceptionServiceInstaller : WindsorInstallerBase
    {
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

            container.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
                .BasedOn<IHttpController>()
                .LifestyleTransient());

            //TODO: do we need this?
            container.Register(Component.For<IHttpControllerActivator>()
                .Instance(new WindsorHttpControllerActivator())
                .LifestyleSingleton());
        }
    }
}


