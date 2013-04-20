using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.IoC;
using Errordite.Reception.Web.Controllers;

namespace Errordite.Reception.Web.IoC
{
    public class ReceptionWebInstaller : WindsorInstallerBase
    {
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);

            container.Register(AllTypes.FromThisAssembly()
                .BasedOn<IController>()
                .If(Component.IsInSameNamespaceAs<ReceiveErrorController>())
                .If(t => t.Name.EndsWith("Controller"))
                .LifestyleTransient());

            new ErrorditeCoreInstaller("Errordite.Reception.Web").Install(container, store);
            new PerWebRequestAppSessionInstaller().Install(container, store);
        }
    }
}