using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Web.Mvc;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Web;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Central;
using Errordite.Core.Monitoring.Commands;
using Errordite.Core.Monitoring.Entities;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Areas.System.Models.Services;
using Errordite.Web.Controllers;
using Newtonsoft.Json;
using Errordite.Web.Extensions;

namespace Errordite.Web.Areas.System.Controllers
{
    public class ServicesController : ErrorditeController
    {
        private readonly IEnumerable<ServiceConfiguration> _serviceConfigurations;
        private readonly IGetRavenInstancesQuery _getRavenInstancesQuery;

        public ServicesController(IGetRavenInstancesQuery getRavenInstancesQuery,
            IEnumerable<ServiceConfiguration> serviceConfigurations)
        {
            _getRavenInstancesQuery = getRavenInstancesQuery;
            _serviceConfigurations = serviceConfigurations;
        }

        [ImportViewData]
        public ActionResult Index(string ravenInstanceId)
        {
            return View(GetSystemStatusViewModel(ravenInstanceId ?? "1"));
        }

        public SystemStatusViewModel GetSystemStatusViewModel(string ravenInstanceId)
        {
            var instances = _getRavenInstancesQuery.Invoke(new GetRavenInstancesRequest()).RavenInstances;

            var model = new SystemStatusViewModel
            {
                Services = new List<ServiceInfoViewModel>(),
                RavenInstances = instances.ToSelectList(r => r.FriendlyId, r => "Instance #{0} (Master: {1} Active: {2})".FormatWith(r.FriendlyId, r.IsMaster, r.Active), r => r.FriendlyId == ravenInstanceId)
            };

            foreach (var service in _serviceConfigurations)
            {
                ServiceConfiguration service1 = service;
                Parallel.ForEach(instances, instance => GetStatusForEndpoint(model, instance, service1));
            }

            return model;
        }

        private void GetStatusForEndpoint(SystemStatusViewModel model, RavenInstance instance, ServiceConfiguration service)
        {
            try
            {
                var response = SynchronousWebRequest
                    .To("{0}:{1}/admin/queue/status".FormatWith(instance.ServiceHttpEndpoint, service.PortNumber))
                    .WithMethod(HttpConstants.HttpMethods.Get)
                    .WithContentType(HttpConstants.ContentTypes.Json)
                    .TimeoutIn(5000)
                    .GetResponse();

                if (response.Status == HttpStatusCode.OK)
                {
                    model.Services.Add(new ServiceInfoViewModel
                    {
                        Configuration = service,
                        ServiceStatus = JsonConvert.DeserializeObject<ServiceStatus>(response.Body),
                        RavenInstance = instance
                    });
                }
                else
                {
                    AddNonRunningEndpoint(model, service);
                }
            }
            catch (WebException)
            {
                AddNonRunningEndpoint(model, service);
            }
        }

        private void AddNonRunningEndpoint(SystemStatusViewModel model, ServiceConfiguration configuration)
        {
            model.Services.Add(new ServiceInfoViewModel
            {
                Configuration = configuration,
                ServiceStatus = new ServiceStatus
                {
                    Status = ServiceControllerStatus.Stopped,
                    ServiceName = configuration.ServiceName,
                    InputQueueStatus = new QueueStatus
                    {
                        QueueName = configuration.QueueName
                    },
                    ErrorQueueStatus = new QueueStatus
                    {
                        QueueName = configuration.ErrorQueueName
                    }
                }
            }); 
        }

        [ImportViewData, HttpPost]
        public ActionResult ReturnToSource(string queueName, string serviceName, string instanceId)
        {
            var instances = _getRavenInstancesQuery.Invoke(new GetRavenInstancesRequest()).RavenInstances;
            var service = _serviceConfigurations.FirstOrDefault(e => e.ServiceName.ToLowerInvariant() == serviceName.ToLowerInvariant());

            if (service == null)
                return new JsonErrorResult("Invalid endpoint, cannot connect to service.");

            var instance = instances.First(i => i.FriendlyId == instanceId);

            try
            {
                var request = new ReturnMessageToSourceQueueRequest
                {
                    ErrorQueue = service.ErrorQueueName,
                    SourceQueue = service.QueueName
                };

                var response = SynchronousWebRequest
                    .To("{0}:{1}/admin/queue/returntosource".FormatWith(instance.ServiceHttpEndpoint, service.PortNumber))
                    .WithMethod(HttpConstants.HttpMethods.Post)
                    .WithContentType(HttpConstants.ContentTypes.Json)
                    .TimeoutIn(30000)
                    .Raw(JsonConvert.SerializeObject(request))
                    .GetResponse();

                if (!response.Status.IsIn(HttpStatusCode.OK, HttpStatusCode.NoContent))
                {
                    return new JsonErrorResult("Invalid response code:{0}".FormatWith(response.Status));
                }
            }
            catch (WebException)
            {
                return new JsonErrorResult("Error failed to connect to service.");
            }

            return new JsonSuccessResult();
        }

        [ImportViewData, HttpPost]
        public ActionResult DeleteMessages(string queueName, string serviceName, string instanceId)
        {
            var instances = _getRavenInstancesQuery.Invoke(new GetRavenInstancesRequest()).RavenInstances;
            var service = _serviceConfigurations.FirstOrDefault(e => e.ServiceName.ToLowerInvariant() == serviceName.ToLowerInvariant());

            if (service == null)
                return new JsonErrorResult("Invalid endpoint, cannot connect to service.");

            var instance = instances.First(i => i.FriendlyId == instanceId);

            try
            {
                var request = new DeleteMessagesRequest
                {
                    ErrorQueue = service.ErrorQueueName
                };

                var response = SynchronousWebRequest
                    .To("{0}:{1}/admin/queue/delete".FormatWith(instance.ServiceHttpEndpoint, service.PortNumber))
                    .WithMethod(HttpConstants.HttpMethods.Post)
                    .WithContentType(HttpConstants.ContentTypes.Json)
                    .TimeoutIn(30000)
                    .Raw(JsonConvert.SerializeObject(request))
                    .GetResponse();

                if (!response.Status.IsIn(HttpStatusCode.OK, HttpStatusCode.NoContent))
                {
                    return new JsonErrorResult("Invalid response code:{0}".FormatWith(response.Status));
                }
            }
            catch (WebException)
            {
                return new JsonErrorResult("Error failed to connect to service.");
            }

            return new JsonSuccessResult();
        }

        [ImportViewData, HttpPost]
        public ActionResult ServiceControl(string serviceName, string machineName, bool start)
        {
            var scm = new ServiceController(serviceName, machineName);

            if (start)
            {
                scm.Start();
                scm.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

                if (scm.Status == ServiceControllerStatus.Running)
                    return new JsonSuccessResult(allowGet: true);
            }
            else
            {
                scm.Stop();
                scm.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

                if (scm.Status == ServiceControllerStatus.Stopped)
                    return new JsonSuccessResult(allowGet: true);
            }

            return new JsonErrorResult(allowGet: true);
        }
    }
}
