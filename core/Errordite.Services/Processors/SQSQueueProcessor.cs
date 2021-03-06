﻿using System;
using System.Collections.Generic;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Client;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Exceptions;
using Errordite.Core.IoC;
using Errordite.Core.Messaging;
using System.Linq;
using Errordite.Core.Messaging.Commands;
using Errordite.Services.Throttlers;
using Newtonsoft.Json;
using Errordite.Core.Extensions;

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
        private readonly ErrorditeConfiguration _configuration;
        private bool _pollNow;

        public string OrganisationFriendlyId { get; private set; }

        public SQSQueueProcessor(IEnumerable<ServiceConfiguration> serviceConfigurations,
            AmazonSQS amazonSQS, 
            IRequestThrottler requestThrottler, 
            ICreateSQSQueueCommand createSQSQueueCommand, 
            ErrorditeConfiguration configuration)
        {
            _serviceConfiguration = serviceConfigurations.First(c => c.IsActive);
            _amazonSQS = amazonSQS;
            _requestThrottler = requestThrottler;
            _createSQSQueueCommand = createSQSQueueCommand;
            _configuration = configuration;
        }

        public void Start(string organisationId, string ravenInstanceId)
        {
            _queueUrl = _configuration.GetQueueForService(_serviceConfiguration.Service, organisationId, ravenInstanceId);

            Trace("Starting SQS Queue Processor for organisation:={0}, ravenInstanceId:={1}, queue:={2}", organisationId ?? string.Empty, ravenInstanceId, _queueUrl);
            OrganisationFriendlyId = organisationId;

            _serviceRunning = true;
            _worker = new Thread(ReceiveFromQueue)
            {
                IsBackground = true
            };
            _worker.Start();
            Trace("Started SQS Queue Processor");
        }

        public void Stop()
        {
            Trace("Stopping SQS Queue Processor for organisation:={0}", OrganisationFriendlyId ?? string.Empty);
            _serviceRunning = false;
            _worker.Abort();
            Trace("Stopped SQS Queue Processor");
        }

		public void StopPolling()
		{
			Trace("Stopping Polling SQS Queue Processor for organisation:={0}", OrganisationFriendlyId ?? string.Empty);
			_serviceRunning = false;
			Trace("Stopped Polling SQS Queue Processor");
		}

        public void PollNow()
        {
            _pollNow = true;
        }

        private void ReceiveFromQueue()
        {
            int emptyReceiptCount = 0;

            while (_serviceRunning)
            {
	            try
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
						//create the queue if the exception indicates the queue does not exist
						if (e.Message.ToLowerInvariant().Contains("the specified queue does not exist"))
						{
							_createSQSQueueCommand.Invoke(new CreateSQSQueueRequest
							{
								QueueName = _queueUrl.GetQueueName()
							});
						}

						emptyReceiptCount = 9; //equiv to 3 mins pause, the maximum we wait
						Thread.Sleep(_requestThrottler.GetDelayMilliseconds(emptyReceiptCount));
						continue;
					}

					if (!response.IsSetReceiveMessageResult() || response.ReceiveMessageResult.Message.Count == 0)
					{
						emptyReceiptCount++;
						Trace("No message returned from queue '{0}', zero message count:={1}", _queueUrl, emptyReceiptCount);

						//sleep for delay as specified by throttler (unless instructed to poll now)
						int delay = _requestThrottler.GetDelayMilliseconds(emptyReceiptCount);
						const int sleepPeriod = 100;
						int sleepCount = 0;
						while (sleepPeriod * ++sleepCount < delay)
						{
							if (_pollNow)
							{
								emptyReceiptCount = 0;
								_pollNow = false;
								break;
							}
							Thread.Sleep(sleepPeriod);
						}
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
							var de = new ErrorditeDeleteSQSMessageException("Failed to delete message with receipt handle:={0}".FormatWith(envelope.ReceiptHandle), true, e);

							if (e is ThreadAbortException)
								continue;

							Error(de);
							ErrorditeClient.ReportException(de);
						}
					}
	            }
	            catch (Exception e)
	            {
		            if (e is ThreadAbortException)
			            continue;

					Error(e);
					ErrorditeClient.ReportException(e);
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
