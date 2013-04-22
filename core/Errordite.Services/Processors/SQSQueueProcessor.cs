using System;
using System.Collections.Generic;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.IoC;
using Errordite.Core.Messaging;
using System.Linq;
using Errordite.Core.Messaging.Commands;
using Errordite.Services.Throttlers;
using Newtonsoft.Json;

namespace Errordite.Services.Processors
{
    public class SQSQueueProcessor : ComponentBase, IQueueProcessor
    {
        private Thread _worker;
        private string _queueUrl;
        private bool _serviceRunning;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly AmazonSQS _amazonSQS;
        private readonly IRequestThrottler _requestThrottler;
        private readonly ICreateSQSQueueCommand _createSQSQueueCommand;

        public string OrganisationId { get; private set; }

        public SQSQueueProcessor(IEnumerable<ServiceConfiguration> serviceConfigurations,
            AmazonSQS amazonSQS, 
            IRequestThrottler requestThrottler, 
            ICreateSQSQueueCommand createSQSQueueCommand)
        {
            _serviceConfiguration = serviceConfigurations.First(c => c.IsActive);
            _amazonSQS = amazonSQS;
            _requestThrottler = requestThrottler;
            _createSQSQueueCommand = createSQSQueueCommand;
        }

        public void Start(string organisationId = null)
        {
            OrganisationId = organisationId;
            _queueUrl = _serviceConfiguration.QueueAddress + organisationId;
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
            _worker.Join(TimeSpan.FromSeconds(5));
            _worker.Abort();
        }

        private void ReceiveFromQueue()
        {
            int emptyReceiptCount = 0;

            while (_serviceRunning)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = _serviceConfiguration.MaxNumberOfMessagesPerReceive,
                    WaitTimeSeconds = 20
                };

                Trace("Attmepting to receive message from queue '{0}'", _queueUrl);

                ReceiveMessageResponse response;

                try
                {
                    response = _amazonSQS.ReceiveMessage(request);
                }
                catch (AmazonSQSException e)
                {
                    //craete the queue if the exception indicates the queue does not exist
                    if (e.Message.ToLowerInvariant().StartsWith("the specified queue does not exist"))
                    {
                        _createSQSQueueCommand.Invoke(new CreateSQSCommandRequest
                        {
                            OrganisationId = OrganisationId
                        });
                    }

                    emptyReceiptCount = 18; //equiv to 3 mins pause, the maximum we wait
                    Thread.Sleep(_requestThrottler.GetDelayMilliseconds(emptyReceiptCount));
                    continue;
                }
                
                if (!response.IsSetReceiveMessageResult() || response.ReceiveMessageResult.Message.Count == 0)
                {
                    emptyReceiptCount++;
                    Trace("No message returned from queue '{0}', zero message count:={1}", _queueUrl, emptyReceiptCount);

                    //sleep for delay as specified by throttler
                    Thread.Sleep(_requestThrottler.GetDelayMilliseconds(emptyReceiptCount));
                    continue;
                }

                //reset the zero message count
                emptyReceiptCount = 0;

                Trace("Received {0} messages from queue '{1}'", response.ReceiveMessageResult.Message.Count, _queueUrl);
                  
                foreach (var message in response.ReceiveMessageResult.Message)
                {
                    var envelope = GetEnvelope(message);

                    Trace("Receiving message for organisation:={0}", envelope.OrganisationId);

                    try
                    {
                        ObjectFactory.GetObject<IMessageProcessor>().Process(_serviceConfiguration, envelope);

                        _amazonSQS.DeleteMessage(new DeleteMessageRequest
                        {
                            QueueUrl = _queueUrl,
                            ReceiptHandle = envelope.ReceiptHandle
                        });
                    }
                    catch (Exception e)
                    {
                        //TODO: how do we handle failures here?
                        Error(e);
                    }
                }
            }
        }

        private MessageEnvelope GetEnvelope(Message message)
        {
            var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(message.Body);
            envelope.ReceiptHandle = message.ReceiptHandle;
            envelope.MessageId = message.MessageId;
            envelope.Service = _serviceConfiguration.Service;
            return envelope;
        }
    }
}
