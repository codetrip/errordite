using System;
using System.Collections.Generic;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Services.Deserialisers;
using Errordite.Services.Processors;
using System.Linq;

namespace Errordite.Services.Queuing
{
    public class SQSQueueProcessor : ComponentBase, IQueueProcessor
    {
        private readonly object _syncLock = new object();
        private readonly AmazonSQS _amazonSQS;
        private bool _serviceRunning;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly List<Thread> _workerThreads = new List<Thread>();
        private readonly IMessageDeserialiser _deserialiser;
        private readonly IList<MessageProcessor> _messageProcessors = new List<MessageProcessor>();

        public SQSQueueProcessor(IEnumerable<ServiceConfiguration> serviceConfigurations,
            IMessageDeserialiser deserialiser,
            AmazonSQS amazonSQS)
        {
            _serviceConfiguration = serviceConfigurations.First(c => c.IsActive);
            _amazonSQS = amazonSQS;
            _deserialiser = deserialiser;
        }

        public void Start()
        {
            _serviceRunning = true;

            for (int threadCount = 0; threadCount < _serviceConfiguration.QueueProcessingThreads; threadCount++)
            {
                var thread = new Thread(ReceiveFromQueue)
                {
                    IsBackground = true
                };

                thread.Start();
                _workerThreads.Add(thread);
            }
        }

        public void Stop()
        {
            _serviceRunning = false;

            foreach (var thread in _workerThreads)
            {
                thread.Join(TimeSpan.FromSeconds(10));
                thread.Abort();
            }
        }

        private void ReceiveFromQueue()
        {
            while (_serviceRunning)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _serviceConfiguration.QueueAddress,
                    MaxNumberOfMessages = _serviceConfiguration.MaxNumberOfMessages,
                    WaitTimeSeconds = 20
                };

                Trace("Attmpeting to receive message from queue:={0}", _serviceConfiguration.QueueAddress);
                var response = _amazonSQS.ReceiveMessage(request);

                if (!response.IsSetReceiveMessageResult())
                {
                    Trace("No message returned");
                    continue;
                }
                  
                foreach (var message in response.ReceiveMessageResult.Message)
                {
                    var envelope = _deserialiser.Deserialise(message.Body);
                    envelope.ReceiptHandle = response.ReceiveMessageResult.Message[0].ReceiptHandle;

                    Trace("Receiving message");

                    var processor = _messageProcessors.FirstOrDefault(p => p.ContainsOrganisation(envelope.OrganisationId));

                    if (processor != null)
                    {
                        Trace("Found processor for organisation {0}", envelope.OrganisationId);
                        processor.Enquque(envelope);
                    }
                    else
                    {
                        lock (_syncLock)
                        {
                            processor = _messageProcessors.FirstOrDefault(p => p.ContainsOrganisation(envelope.OrganisationId));

                            if (processor != null)
                            {
                                processor.Enquque(envelope);
                            }
                            else
                            {
                                processor = _messageProcessors.FirstOrDefault(p => p.CanAddOrganisation());

                                if (processor != null)
                                {
                                    processor.AddOrganisation(envelope.OrganisationId);
                                    processor.Enquque(envelope);
                                }
                                else
                                {
                                    processor = new MessageProcessor(_serviceConfiguration, _amazonSQS, Auditor);
                                    processor.AddOrganisation(envelope.OrganisationId);
                                    processor.Enquque(envelope);

                                    _messageProcessors.Add(processor);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
