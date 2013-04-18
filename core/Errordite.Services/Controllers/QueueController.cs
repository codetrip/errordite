//using System.Collections.Generic;
//using System.Linq;
//using Errordite.Core.Monitoring;
//using Errordite.Core.Monitoring.Commands;
//using Errordite.Core.Monitoring.Queries;
//using Errordite.Services.Configuration;

//namespace Errordite.Services.Controllers
//{
//    public class QueueController : AbstractQueueController
//    {
//        private readonly IEnumerable<ServiceConfiguration> _serviceConfigurations;

//        public QueueController(IGetQueueStatusQuery getQueueStatusQuery, 
//            IDeleteMessagesCommand deleteQueueMessages, 
//            IReturnMessageToSourceQueueCommand returnToSourceCommand,
//            IEnumerable<ServiceConfiguration> serviceConfigurations) : 
//            base(getQueueStatusQuery, deleteQueueMessages, returnToSourceCommand)
//        {
//            _serviceConfigurations = serviceConfigurations;
//        }

//        protected override ServiceConfiguration GetServiceConfiguration()
//        {
//            return _serviceConfigurations.First(c => c.ServiceName.ToLowerInvariant() == "errorditereceptionservice");
//        }
//    }
//}
