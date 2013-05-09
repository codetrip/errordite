using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.IoC;
using Errordite.Receive.Controllers;

namespace Errordite.Receive.IoC
{
    public class ReceiveInstaller : WindsorInstallerBase
    {
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

            container.Register(AllTypes.FromThisAssembly()
                .BasedOn<IController>()
                .If(Component.IsInSameNamespaceAs<ReceiveErrorController>())
                .If(t => t.Name.EndsWith("Controller"))
                .LifestyleTransient());

            new ErrorditeCoreInstaller("Errordite.Receive.Web").Install(container, store);
            new PerWebRequestAppSessionInstaller().Install(container, store);
        }
    }
}