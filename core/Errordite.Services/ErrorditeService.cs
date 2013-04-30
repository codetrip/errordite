using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Master;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using Errordite.Core.IoC;
using Errordite.Core.Session;
using Errordite.Core.Web;
using Errordite.Services.Processors;
using Raven.Client;
using log4net.Config;
using System.Linq;
using Castle.MicroKernel.Lifestyle;
using Raven.Client.Linq;

namespace Errordite.Services
{
    public interface IErrorditeService
    {
        void Start(string ravenInstanceId);
        void Stop();
        void Configure();
        void AddOrganisation(Organisation organisation);
        void RemoveOrganisation(string organisationId);
        void PollNow(string organisationFriendlyId);
    }

    public class ErrorditeService : ComponentBase, IErrorditeService
    {
        private readonly IList<IQueueProcessor> _queueProcessors = new List<IQueueProcessor>();
        private readonly ServiceConfiguration _serviceConfiguration;

        public ErrorditeService(IEnumerable<ServiceConfiguration> serviceConfigurations)
        {
            _serviceConfiguration = serviceConfigurations.First(c => c.IsActive);
        }

        public void PollNow(string organisationFriendlyId)
		{
			Trace("Poll now for organisation:={0}", organisationFriendlyId);

            var processor = _queueProcessors.FirstOrDefault(q => q.OrganisationFriendlyId == organisationFriendlyId);

            if(processor != null)
                processor.PollNow();
        }

        public void Start(string ravenInstanceId)
        {
            Trace("Starting Errordite service {0} for raven instance:={1}", _serviceConfiguration.Service, ravenInstanceId);

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
                            .Query<OrganisationDocument, Organisations>()
                            .Where(o => o.RavenInstanceId == RavenInstance.GetId(ravenInstanceId))
							.As<Organisation>()
                            .ToList();
                    }
                }

                foreach (var organisation in organisations)
                {
                    AddProcessor(organisation.FriendlyId, null);
                }
            }
            else
            {
                for (int i = 0; i < _serviceConfiguration.ServiceProcessorCount; i++)
                {
                    AddProcessor(null, ravenInstanceId);
                }
            }

            Trace("Started Errordite service {0} for raven instance:={1}", _serviceConfiguration.Service, ravenInstanceId);
        }

        public void AddOrganisation(Organisation organisation)
        {
            Trace("Adding SQS Queue Processor for organisation:={0}", organisation.FriendlyId);
            if (_queueProcessors.All(p => p.OrganisationFriendlyId != organisation.FriendlyId))
            {
                AddProcessor(organisation.FriendlyId, null);
                Trace("Added SQS Queue Processor for organisation:={0}", organisation.FriendlyId);
            }
            else
            {
                Trace("SQS Queue Processor for organisation:={0} already exists", organisation.FriendlyId);
            }
        }

        public void RemoveOrganisation(string organisationId)
        {
            Trace("Removing SQS Queue Processor for organisation:={0}", organisationId);
            var processor = _queueProcessors.FirstOrDefault(p => p.OrganisationFriendlyId != organisationId.GetFriendlyId());
            if (processor != null)
            {
                processor.Stop();
                _queueProcessors.Remove(processor);
            }
            Trace("Removed SQS Queue Processor for organisation:={0}", organisationId);
        }

        public void AddProcessor(string organisationId, string ravenInstanceId)
        {
            Trace("Adding SQS Queue Processor for organisation:={0}", organisationId ?? string.Empty);
            var processor = ObjectFactory.GetObject<IQueueProcessor>();
            _queueProcessors.Add(processor);
            processor.Start(organisationId, ravenInstanceId);
            Trace("Successfully added SQS Queue Processor for organisation:={0}", organisationId ?? string.Empty);
        }

        public void Stop()
        {
            Trace("Stopping SQS Queue Processors");

            foreach (var processor in _queueProcessors)
            {
                processor.Stop();
            }

            ObjectFactory.Container.Dispose();

            Trace("Stopped SQS Queue Processors");
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
