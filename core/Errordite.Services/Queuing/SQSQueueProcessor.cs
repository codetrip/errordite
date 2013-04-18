using System;
using System.Collections.Generic;
using System.Threading;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using CodeTrip.Core.IoC;
using Errordite.Services.Configuration;
using Errordite.Services.Serialisers;

namespace Errordite.Services.Queuing
{
    public class SQSQueueProcessor
    {
        private readonly AmazonSQS _amazonSQS;
        private bool _serviceRunning;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly List<Thread> _workerThreads = new List<Thread>();
        private readonly IMessageSerialiser _serialiser;
        private readonly 

        public SQSQueueProcessor(ServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration; 
            _amazonSQS = AWSClientFactory.CreateAmazonSQSClient(
                 serviceConfiguration.AWSAccessKey,
                 serviceConfiguration.AWSSecretKey,
                 RegionEndpoint.EUWest1);
            _serialiser = ObjectFactory.GetObject<IMessageSerialiser>(_serviceConfiguration.Service.ToString());
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

                if (response.IsSetReceiveMessageResult())
                {
                    foreach (var message in response.ReceiveMessageResult.Message)
                    {
                        var deserialisedMessage = _serialiser.Deserialise(message.Body);
                    }
                }

                var messageRecieptHandle = response.ReceiveMessageResult.Message[0].ReceiptHandle;
                var deleteRequest = new DeleteMessageRequest
                {
                    QueueUrl = _serviceConfiguration.QueueAddress,
                    ReceiptHandle = messageRecieptHandle
                };

                _amazonSQS.DeleteMessage(deleteRequest);
            }
        }
    }
}
