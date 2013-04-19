using System;
using System.Collections.Generic;
using System.Threading;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Services.Configuration;
using Errordite.Services.Processors;
using Errordite.Services.Serialisers;
using System.Linq;

namespace Errordite.Services.Queuing
{
    public interface IQueueProcessor
    {
        void Start();
        void Stop();
    }

    public class SQSQueueProcessor : IQueueProcessor
    {
        private readonly object _syncLock = new object();
        private readonly AmazonSQS _amazonSQS;
        private bool _serviceRunning;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly List<Thread> _workerThreads = new List<Thread>();
        private readonly IMessageSerialiser _serialiser;
        private readonly IList<MessageProcessor> _messageProcessors = new List<MessageProcessor>(); 

        public SQSQueueProcessor(ServiceConfigurationContainer serviceConfigurationContainer, IEnumerable<IMessageSerialiser> serialisers)
        {
            _serviceConfiguration = serviceConfigurationContainer.Configuration; 
            _amazonSQS = AWSClientFactory.CreateAmazonSQSClient(
                 serviceConfigurationContainer.Configuration.AWSAccessKey,
                 serviceConfigurationContainer.Configuration.AWSSecretKey,
                 RegionEndpoint.EUWest1);
            _serialiser = serialisers.First(s => s.ForService == serviceConfigurationContainer.Instance);
        }

        public void Start()
        {
            _serviceRunning = true;

            for (int concurrentThreadCount = 0; concurrentThreadCount < _serviceConfiguration.Threads; concurrentThreadCount++)
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
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 20
                };

                var response = _amazonSQS.ReceiveMessage(request);

                if (!response.IsSetReceiveMessageResult()) 
                    continue;

                foreach (var message in response.ReceiveMessageResult.Message)
                {
                    var envelope = _serialiser.Deserialise(message.Body);
                    envelope.ReceiptHandle = response.ReceiveMessageResult.Message[0].ReceiptHandle;

                    var processor = _messageProcessors.FirstOrDefault(p => p.ContainsOrganisation(envelope.Message.OrganisationId));

                    if (processor != null)
                    {
                        processor.Enquque(envelope);
                    }
                    else
                    {
                        lock (_syncLock)
                        {
                            processor = _messageProcessors.FirstOrDefault(p => p.ContainsOrganisation(envelope.Message.OrganisationId));

                            if (processor != null)
                            {
                                processor.Enquque(envelope);
                            }
                            else
                            {
                                processor = _messageProcessors.FirstOrDefault(p => p.CanAddOrganisation());

                                if (processor != null)
                                {
                                    processor.AddOrganisation(envelope.Message.OrganisationId);
                                    processor.Enquque(envelope);
                                }
                                else
                                {
                                    processor = new MessageProcessor(_serviceConfiguration, _amazonSQS);
                                    processor.AddOrganisation(envelope.Message.OrganisationId);
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
