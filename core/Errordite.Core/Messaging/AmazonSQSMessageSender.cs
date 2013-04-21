using System;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core.Domain.Organisation;
using Newtonsoft.Json;
using Errordite.Core.Extensions;

namespace Errordite.Core.Messaging
{
    public class AmazonSQSMessageSender : ComponentBase, IMessageSender
    {
        private readonly AmazonSQS _amazonSQS;

        public AmazonSQSMessageSender(AmazonSQS amazonSQS)
        {
            _amazonSQS = amazonSQS;
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

            _amazonSQS.SendMessage(new SendMessageRequest
            {
                QueueUrl = destination,
                MessageBody = JsonConvert.SerializeObject(envelope),
            });
        }
    }
}