using System;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Messaging.Commands;
using Newtonsoft.Json;
using Errordite.Core.Extensions;

namespace Errordite.Core.Messaging
{
    public class AmazonSQSMessageSender : ComponentBase, IMessageSender
    {
        private readonly AmazonSQS _amazonSQS;
        private readonly ICreateSQSQueueCommand _createSQSQueueCommand;

        public AmazonSQSMessageSender(AmazonSQS amazonSQS, ICreateSQSQueueCommand createSQSQueueCommand)
        {
            _amazonSQS = amazonSQS;
            _createSQSQueueCommand = createSQSQueueCommand;
        }

        public void Send<T>(T message, string destination) where T : MessageBase
        {
            var envelope = new MessageEnvelope
            {
                Message = JsonConvert.SerializeObject(message),
                MessageType = message.GetType().AssemblyQualifiedName,
                OrganisationId = message.OrganisationId.IsNullOrEmpty() ? Organisation.NullOrganisationId : message.OrganisationId,
                QueueUrl = destination,
                GeneratedOnUtc = DateTime.UtcNow
            };

            try
            {
                _amazonSQS.SendMessage(new SendMessageRequest
                {
                    QueueUrl = destination,
                    MessageBody = JsonConvert.SerializeObject(envelope),
                });
            }
            catch (AmazonSQSException e)
            {
                //if we are attempting to send a message to a receive queue which does not exist for an organisation
                //craete the queue and try to send the message again
                if (message.OrganisationId.IsNotNullOrEmpty() && 
                    destination.Contains("errordite-receive-") && 
                    e.Message.ToLowerInvariant().StartsWith("the specified queue does not exist"))
                {
                    _createSQSQueueCommand.Invoke(new CreateSQSQueueRequest
                    {
                        OrganisationId = message.OrganisationId
                    });

                    _amazonSQS.SendMessage(new SendMessageRequest
                    {
                        QueueUrl = destination,
                        MessageBody = JsonConvert.SerializeObject(envelope),
                    });
                }
            }
        }
    }
}