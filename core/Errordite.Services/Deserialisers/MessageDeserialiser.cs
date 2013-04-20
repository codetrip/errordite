using Errordite.Core.Messaging;
using Newtonsoft.Json;

namespace Errordite.Services.Deserialisers
{
    public class MessageDeserialiser : IMessageDeserialiser
    {
        public MessageEnvelope Deserialise(string message)
        {
            return JsonConvert.DeserializeObject<MessageEnvelope>(message);
        }
    }
}