using System;
using System.Collections.Generic;
using Amazon.SQS;
using Amazon.SQS.Model;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Queueing;
using System.Linq;
using Errordite.Services.Configuration;
using Errordite.Services.Consumers;
using Errordite.Services.Entities;

namespace Errordite.Services.Processors
{
    public class MessageProcessor
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
            try
            {
                //todo: session scope, transation??
                var consumer = ObjectFactory.GetObject<IErrorditeConsumer>(_serviceConfiguration.Instance.ToString());
                consumer.Consume(envelope.Message);

                var messageRecieptHandle = envelope.ReceiptHandle;
                var deleteRequest = new DeleteMessageRequest
                {
                    QueueUrl = _serviceConfiguration.QueueAddress,
                    ReceiptHandle = messageRecieptHandle
                };

                _amazonSQS.DeleteMessage(deleteRequest);
            }
            catch (Exception e)
            {
                
                throw;
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
