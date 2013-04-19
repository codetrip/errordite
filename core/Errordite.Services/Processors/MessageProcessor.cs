using System;
using System.Collections.Generic;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core;
using Errordite.Core.IoC;
using Errordite.Core.Queueing;
using System.Linq;
using Errordite.Core.Session;
using Errordite.Services.Configuration;
using Errordite.Services.Consumers;
using Errordite.Services.Entities;
using Castle.MicroKernel.Lifestyle;

namespace Errordite.Services.Processors
{
    public class MessageProcessor : ComponentBase
    {
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly List<string> _organisations = new List<string>();
        private readonly OwnThreadQueueHelper<MessageEnvelope> _queueHelper;
        private readonly AmazonSQS _amazonSQS;

        public MessageProcessor(ServiceConfiguration serviceConfiguration, AmazonSQS amazonSQS)
        {
            _serviceConfiguration = serviceConfiguration;
            _amazonSQS = amazonSQS;
            _queueHelper = new OwnThreadQueueHelper<MessageEnvelope>(ProcessMessage);
        }

        private void ProcessMessage(MessageEnvelope envelope)
        {
            bool processed = false;

            for (int attempt = 0; attempt < _serviceConfiguration.RetryLimit; attempt++)
            {
                try
                {
                    TryProcessMessage(envelope);
                    processed = true;
                    break;
                }
                catch (Exception ex)
                {
                    Error(ex);
                }

                if(_serviceConfiguration.RetryDelayMilliseconds > 0)
                    Thread.Sleep(_serviceConfiguration.RetryDelayMilliseconds);
            }
            if (!processed)
            {
                //TODO: handle failures
            }

            _amazonSQS.DeleteMessage(new DeleteMessageRequest
            {
                QueueUrl = _serviceConfiguration.QueueAddress,
                ReceiptHandle = envelope.ReceiptHandle
            });
        }

        private void TryProcessMessage(MessageEnvelope envelope)
        {
            //start a scope on the container so the session is shared only within the context of this message processing routine
            using (ObjectFactory.Container.BeginScope())
            {
                using (var session = ObjectFactory.GetObject<IAppSession>())
                {
                    Trace("Received Message of type {0}", envelope.Message.GetType().FullName);
                    TraceObject(envelope.Message);

                    var messageType = typeof(IErrorditeConsumer<>).MakeGenericType(envelope.Message.GetType());
                    dynamic consumer = ObjectFactory.Container.Resolve(messageType);
                    consumer.Consume((dynamic)envelope.Message);

                    session.Commit();
                }
            }
        }

        public bool ContainsOrganisation(string organisationId)
        {
            return _organisations.Any(o => o == organisationId);
        }

        public bool CanAddOrganisation()
        {
            return _organisations.Count < _serviceConfiguration.MaxOrganisationsPerMessageProcesor;
        }

        public void AddOrganisation(string organisationId)
        {
            _organisations.Add(organisationId);
        }

        public void Enquque(MessageEnvelope message)
        {
            _queueHelper.Enqueue(message);
        }
    }
}
