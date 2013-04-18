
using System;
using System.Collections.Generic;
using System.Threading;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Errordite.Services
{
    public interface IErrorditeConsumer
    { }

    public class ReceptionErrorditeConsumer : IErrorditeConsumer
    {

    }

    public abstract class ErrorditeConsumerBase
    {
        private bool _serviceRunning;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly List<Thread> _workerThreads = new List<Thread>();
        private readonly AmazonSQS _amazonSQS;

        protected ErrorditeConsumerBase(ServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
            _amazonSQS = AWSClientFactory.CreateAmazonSQSClient(
                serviceConfiguration.AWSAccessKey,
                serviceConfiguration.AWSSecretKey,
                RegionEndpoint.EUWest1);
        }

        public void Start()
        {
            _serviceRunning = true;

            for (int concurrentThreadCount = 0;
                 concurrentThreadCount < _serviceConfiguration.Threads;
                 concurrentThreadCount++)
            {
                var thread = new Thread(QueueProcessor);
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

        private void QueueProcessor()
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
