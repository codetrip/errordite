using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Web.Http;
using Errordite.Core.Configuration;
using Errordite.Core.Monitoring.Commands;
using Errordite.Core.Monitoring.Entities;
using Errordite.Core.Monitoring.Queries;

namespace Errordite.Core.Monitoring
{
    public abstract class AbstractQueueController : ApiController
    {
        private readonly IGetQueueStatusQuery _getQueueStatusQuery;
        private readonly IDeleteMessagesCommand _deleteQueueMessages;
        private readonly IReturnMessageToSourceQueueCommand _returnToSourceCommand;

        protected abstract ServiceConfiguration GetServiceConfiguration();

        protected AbstractQueueController(IGetQueueStatusQuery getQueueStatusQuery, 
            IDeleteMessagesCommand deleteQueueMessages, 
            IReturnMessageToSourceQueueCommand returnToSourceCommand)
        {
            _getQueueStatusQuery = getQueueStatusQuery;
            _deleteQueueMessages = deleteQueueMessages;
            _returnToSourceCommand = returnToSourceCommand;
        }

        [HttpGet]
        public ServiceStatus Status()
        {
            var config = GetServiceConfiguration();
            var serviceController = new ServiceController(config.ServiceName, Environment.MachineName);

            var serviceStatus = new ServiceStatus
            {
                ServiceName = config.ServiceName,
                Status = serviceController.Status,
                ProcessId = Process.GetCurrentProcess().Id,
                ErrorQueueStatus = _getQueueStatusQuery.Invoke(new GetQueueStatusRequest
                {
                    QueuePath = config.FullyQualifiedErrorQueueName
                }).Status,
                InputQueueStatus = _getQueueStatusQuery.Invoke(new GetQueueStatusRequest
                {
                    QueuePath = config.FullyQualifiedQueueName
                }).Status
            };

            return serviceStatus;
        }

        [HttpPost]
        public void Delete(DeleteMessagesRequest request)
        {
            _deleteQueueMessages.Invoke(request);
        }

        [HttpPost]
        public void ReturnToSource(ReturnMessageToSourceQueueRequest request)
        {
            _returnToSourceCommand.Invoke(request);
        }
    }
}