using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Errordite.Core.IoC;
using Errordite.Services.Consumers;
using Errordite.Services.Deserialisers;
using Errordite.Services.Queuing;

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

            container.Register(Component.For<IErrorditeService>()
                .ImplementedBy(typeof(ErrorditeService))
                .LifestyleSingleton());

            container.Register(Component.For<IHttpControllerActivator>()
                .Instance(new WindsorHttpControllerActivator())
                .LifestyleSingleton());

            container.Register(Component.For<IMessageDeserialiser>()
               .ImplementedBy(typeof(AmazonSQSMessageDeserialiser))
               .LifestyleTransient());

            container.Register(Component.For<IQueueProcessor>()
               .ImplementedBy(typeof(SQSQueueProcessor))
               .LifestyleTransient());

            container.Register(Classes.FromAssembly(Assembly.GetExecutingAssembly())
                .BasedOn(typeof (IErrorditeConsumer<>))
                .WithServiceFromInterface(typeof (IErrorditeConsumer<>))
                .LifestyleTransient());

            container.Register(Classes.FromAssembly(Assembly.GetExecutingAssembly())
                .BasedOn(typeof(IMessageDeserialiser))
                .WithServiceFromInterface(typeof(IMessageDeserialiser))
                .LifestyleTransient());
        }
    }
}


