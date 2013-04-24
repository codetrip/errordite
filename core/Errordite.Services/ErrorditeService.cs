using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using Errordite.Core.IoC;
using Errordite.Core.Session;
using Errordite.Core.Web;
using Errordite.Services.Processors;
using log4net.Config;
using System.Linq;
using Castle.MicroKernel.Lifestyle;

namespace Errordite.Services
{
    public interface IErrorditeService
    {
        void Start(string ravenInstanceId);
        void Stop();
        void Configure();
        void AddOrganisation(Organisation organisation);
        void RemoveOrganisation(string organisationId);
    }

    public class ErrorditeService : IErrorditeService
    {
        private readonly IList<IQueueProcessor> _queueProcessors = new List<IQueueProcessor>();
        private readonly ServiceConfiguration _serviceConfiguration;

        public ErrorditeService(IEnumerable<ServiceConfiguration> serviceConfigurations)
        {
            _serviceConfiguration = serviceConfigurations.First(c => c.IsActive);
        }

        public void Start(string ravenInstanceId)
        {
            //receive service runs a thread per organisation, polling every org's queue
            //other services just have a single thread processing the queue
            if (_serviceConfiguration.Service == Service.Receive)
            {
                IEnumerable<Organisation> organisations;
                using (ObjectFactory.Container.Kernel.BeginScope())
                {
                    using (var session = ObjectFactory.GetObject<IAppSession>())
                    {
                        organisations = session.MasterRaven
                            .Query<Organisation, Organisations_Search>()
                            .Where(o => o.RavenInstanceId == ravenInstanceId)
                            .ToList();
                    }
                }

                foreach (var organisation in organisations)
                {
                    AddProcessor(organisation.FriendlyId);
                }
            }
            else
            {
                for (int i = 0; i < _serviceConfiguration.ServiceProcessorCount; i++)
                {
                    AddProcessor();
                }
            }
        }

        public void AddOrganisation(Organisation organisation)
        {
            if (_queueProcessors.All(p => p.OrganisationId != organisation.FriendlyId))
            {
                AddProcessor(organisation.FriendlyId);
            }
        }

        public void RemoveOrganisation(string organisationId)
        {
            var processor = _queueProcessors.FirstOrDefault(p => p.OrganisationId != organisationId.GetFriendlyId());
            if (processor != null)
            {
                processor.Stop();
                _queueProcessors.Remove(processor);
            }
        }

        public void AddProcessor(string organisationId = null)
        {
            var processor = ObjectFactory.GetObject<IQueueProcessor>();
            _queueProcessors.Add(processor);
            processor.Start(organisationId);
        }

        public void Stop()
        {
            foreach (var processor in _queueProcessors)
                processor.Stop();

            ObjectFactory.Container.Dispose();
        }

        public void Configure()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\log4net.config")));

            var config = new HttpSelfHostConfiguration("http://localhost:{0}".FormatWith(_serviceConfiguration.PortNumber));

            config.Services.Replace(typeof(IHttpControllerActivator), new WindsorHttpControllerActivator());
            config.DependencyResolver = new WindsorDependencyResolver(ObjectFactory.Container);
            config.MaxReceivedMessageSize = 655360;
            config.MaxBufferSize = 655360;
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.Formatters.JsonFormatter.SerializerSettings = WebApiSettings.JsonSerializerSettings;

            config.Routes.MapHttpRoute(
                name: "api",
                routeTemplate: "api/{orgid}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();

            Console.WriteLine("The server is running on endpoint http://localhost:{0}...".FormatWith(_serviceConfiguration.PortNumber));
        }
    }
}
