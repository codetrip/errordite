using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.IoC;
using Errordite.Core.IoC;

namespace Errordite.Web.IoC
{
    public class WebInstaller : WindsorInstallerBase
    {
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

            container.Register(AllTypes.FromThisAssembly()
                .BasedOn<IController>()
                .If(t => t.Name.EndsWith("Controller"))
                .LifestyleTransient());

            container.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
                .BasedOn<IHttpController>()
                .LifestyleTransient());

            container.Register(AllTypes.FromThisAssembly()
                .BasedOn<IMappingDefinition>()
                .WithServiceFromInterface(typeof(IMappingDefinition))
                .LifestyleTransient());

            container.Install(new CoreInstaller("Errordite.Web"));
            container.Install(new ErrorditeCoreInstaller());
            container.Install(new ErrorditeNServiceBusClientInstaller());
            container.Install(new PerWebRequestAppSessionInstaller());
        }
    }
}