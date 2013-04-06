using System;
using System.IO;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.IoC;
using Errordite.Core.Configuration;
using Errordite.Core.IoC;
using Errordite.Core.WebApi;
using Errordite.Notifications.Service.IoC;
using log4net.Config;
using NServiceBus;

namespace Errordite.Notifications.Service
{
    public class EndpointConfiguration : IConfigureThisEndpoint, AsA_Server, IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance.DefineEndpointName(typeof(EndpointConfiguration).Namespace);
            SetLoggingLibrary.Log4Net(XmlConfigurator.Configure);
            ObjectFactory.Container.Install(new NotificationsMasterInstaller());
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\log4net.config")));

            var serviceConfiguration = ObjectFactory.GetObject<ServiceConfiguration>("NotificationsServiceConfiguration");
            var config = new HttpSelfHostConfiguration("http://localhost:{0}".FormatWith(serviceConfiguration.PortNumber));

            config.Services.Replace(typeof(IHttpControllerActivator), new WindsorHttpControllerActivator());
            config.DependencyResolver = new WindsorDependencyResolver(ObjectFactory.Container);
            config.MaxReceivedMessageSize = 655360;
            config.MaxBufferSize = 655360;
            //this has the effect of always defaulting to Json serialization as there are no Xml formatters registered
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            config.Formatters.JsonFormatter.SerializerSettings = WebApiSettings.JsonSerializerSettings;
            config.Routes.MapHttpRoute(
                name: "admin",
                routeTemplate: "admin/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
            Console.WriteLine("The server is running on endpoint http://localhost:{0}...".FormatWith(serviceConfiguration.PortNumber));
        }
    }
}