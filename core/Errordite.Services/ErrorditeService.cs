using System;
using System.IO;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.IoC;
using Errordite.Core.IoC;
using Errordite.Core.WebApi;
using Errordite.Services.Configuration;
using Errordite.Services.IoC;
using Errordite.Services.Queuing;
using log4net.Config;

namespace Errordite.Services
{
    public interface IErrorditeService
    {
        void Start();
        void Stop();
    }

    public class ErrorditeService : IErrorditeService
    {
        private readonly IQueueProcessor _queueProcessor;
        private readonly ServiceConfigurationContainer _serviceConfigurationContainer;

        public ErrorditeService(ServiceConfigurationContainer serviceConfigurationContainer, IQueueProcessor queueProcessor)
        {
            _serviceConfigurationContainer = serviceConfigurationContainer;
            _queueProcessor = queueProcessor;
        }

        public void Start()
        {
            Configure();
            _queueProcessor.Start();
        }

        public void Stop()
        {
            _queueProcessor.Stop();
        }

        private void Configure()
        {
            ObjectFactory.Container.Install(new ServicesMasterInstaller(_serviceConfigurationContainer.Instance));
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\log4net.config")));

            var config = new HttpSelfHostConfiguration("http://localhost:{0}".FormatWith(_serviceConfigurationContainer.Configuration.PortNumber));

            config.Services.Replace(typeof(IHttpControllerActivator), new WindsorHttpControllerActivator());
            config.DependencyResolver = new WindsorDependencyResolver(ObjectFactory.Container);
            config.MaxReceivedMessageSize = 655360;
            config.MaxBufferSize = 655360;
            //this has the effect of always defaulting to Json serialization as there are no Xml formatters registered
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.Formatters.JsonFormatter.SerializerSettings = WebApiSettings.JsonSerializerSettings;

            config.Routes.MapHttpRoute(
                name: "issueapi",
                routeTemplate: "api/{orgid}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "admin",
                routeTemplate: "admin/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();

            Console.WriteLine("The server is running on endpoint http://localhost:{0}...".FormatWith(_serviceConfigurationContainer.Configuration.PortNumber));
        }
    }
}
