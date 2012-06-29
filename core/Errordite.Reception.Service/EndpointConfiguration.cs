using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using Errordite.Core.Configuration;
using Errordite.Reception.Service.IoC;
using log4net.Config;
using NServiceBus;
using CodeTrip.Core.Extensions;

namespace Errordite.Reception.Service
{
    public class EndpointConfiguration : IConfigureThisEndpoint, AsA_Server, IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance.DefineEndpointName(typeof(EndpointConfiguration).Namespace);
            SetLoggingLibrary.Log4Net(XmlConfigurator.Configure);
            ObjectFactory.Container.Install(new ReceptionMasterInstaller());
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\log4net.config")));

            var httpConfig = ObjectFactory.GetObject<HttpServerConfiguration>();
            var config = new HttpSelfHostConfiguration(httpConfig.Endpoint);

            config.MaxReceivedMessageSize = 655360;
            config.MaxBufferSize = 655360;
            //this has the effect of always defaulting to Json serialization as there are no Xml formatters registered
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.Routes.MapHttpRoute(
                name: "issueapi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
            Console.WriteLine("The server is running on endpoint {0}...".FormatWith(httpConfig.Endpoint));

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                try
                {
                    ObjectFactory.GetObject<IComponentAuditor>().Error(GetType(), args.Exception);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.Write(e);
                }

                args.SetObserved();
            };
        }
    }
}