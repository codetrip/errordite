using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Amazon.SQS;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.IoC;
using Errordite.Services.Configuration;
using Errordite.Services.Consumers;
using Errordite.Services.Queuing;
using Errordite.Services.Serialisers;

namespace Errordite.Services.IoC
{
    public class ServicesInstaller : WindsorInstallerBase
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

            container.Register(Component.For<IMessageSerialiser>()
               .ImplementedBy(typeof(ReceiveErrorMessageSerialiser))
               .Named(ServiceInstance.Reception.ToString())
               .LifestyleTransient());

            container.Register(Component.For<IQueueProcessor>()
               .ImplementedBy(typeof(SQSQueueProcessor))
               .LifestyleTransient());

            container.Register(Component.For<AmazonSQS>()
                .ImplementedBy<AmazonSQS>()
                .UsingFactoryMethod(kernel => kernel.Resolve<IAmazonSQSFactory>().Create()).LifeStyle.Singleton);

            container.Register(Classes.FromAssembly(Assembly.GetExecutingAssembly())
                .BasedOn(typeof (IErrorditeConsumer<>))
                .WithServiceFromInterface(typeof (IErrorditeConsumer<>))
                .LifestyleTransient());
        }
    }
}


