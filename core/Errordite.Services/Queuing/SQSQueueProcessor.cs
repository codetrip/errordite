using System;
using System.Collections.Generic;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Services.Deserialisers;
using System.Linq;
using Magnum.Extensions;

namespace Errordite.Services.Queuing
{
    public class SQSQueueProcessor : ComponentBase, IQueueProcessor
    {
        private Thread _worker;
        private string _organisationId;
        private bool _serviceRunning;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly IMessageDeserialiser _deserialiser;
        private readonly AmazonSQS _amazonSQS;

        public SQSQueueProcessor(IEnumerable<ServiceConfiguration> serviceConfigurations,
            IMessageDeserialiser deserialiser,
            AmazonSQS amazonSQS)
        {
            _serviceConfiguration = serviceConfigurations.First(c => c.IsActive);
            _amazonSQS = amazonSQS;
            _deserialiser = deserialiser;
        }

        public void Start(string organisationId = null)
        {
            _organisationId = organisationId;
            _serviceRunning = true;
            _worker = new Thread(ReceiveFromQueue)
            {
                IsBackground = true
            };
            _worker.Start();
        }

        public void Stop()
        {
            _serviceRunning = false;
            _worker.Join(TimeSpan.FromSeconds(10));
            _worker.Abort();
        }

        private void ReceiveFromQueue()
        {
            while (_serviceRunning)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _serviceConfiguration.QueueAddress.FormatWith(_organisationId ?? string.Empty),
                    MaxNumberOfMessages = _serviceConfiguration.MaxNumberOfMessages,
                    WaitTimeSeconds = 20
                };

                Trace("Attmepting to receive message from queue:={0}", _serviceConfiguration.QueueAddress);
                var response = _amazonSQS.ReceiveMessage(request);

                if (!response.IsSetReceiveMessageResult() || response.ReceiveMessageResult.Message.Count == 0)
                {
                    Trace("No message returned");
                    continue;
                }
                  
                foreach (var message in response.ReceiveMessageResult.Message)
                {
                    var envelope = _deserialiser.Deserialise(message);

                    Trace("Receiving message for organisation:={0}", envelope.OrganisationId);

                    //var processor = _messageProcessors.FirstOrDefault(p => p.ContainsOrganisation(envelope.OrganisationId));

                    //if (processor != null)
                    //{
                    //    Trace("Found processor for organisation {0}", envelope.OrganisationId);
                    //    processor.Enquque(envelope);
                    //}
                    //else
                    //{
                    //    lock (_syncLock)
                    //    {
                    //        processor = _messageProcessors.FirstOrDefault(p => p.ContainsOrganisation(envelope.OrganisationId));

                    //        if (processor != null)
                    //        {
                    //            processor.Enquque(envelope);
                    //        }
                    //        else
                    //        {
                    //            processor = _messageProcessors.FirstOrDefault(p => p.CanAddOrganisation());

                    //            if (processor != null)
                    //            {
                    //                processor.AddOrganisation(envelope.OrganisationId);
                    //                processor.Enquque(envelope);
                    //            }
                    //            else
                    //            {
                    //                processor = new MessageProcessor(_serviceConfiguration, _amazonSQS, Auditor);
                    //                processor.AddOrganisation(envelope.OrganisationId);
                    //                processor.Enquque(envelope);

                    //                _messageProcessors.Add(processor);
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
        }
    }
}
