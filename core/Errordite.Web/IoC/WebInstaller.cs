using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.Interfaces;
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

            container.Install(new ErrorditeCoreInstaller("Errordite.Web"));
            container.Install(new PerWebRequestAppSessionInstaller());
        }
    }
}