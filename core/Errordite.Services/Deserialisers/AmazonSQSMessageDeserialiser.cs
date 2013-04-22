using Amazon.SQS.Model;
using Errordite.Core.Messaging;
using Newtonsoft.Json;

namespace Errordite.Services.Deserialisers
{
    public class AmazonSQSMessageDeserialiser : IMessageDeserialiser
    {
        public MessageEnvelope Deserialise(Message message)
        {
            var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(message.Body);
            envelope.ReceiptHandle = message.ReceiptHandle;
            envelope.MessageId = message.MessageId;
            return envelope;
        }
    }
}