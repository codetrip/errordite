
using Errordite.Core.Messages;
using Newtonsoft.Json;

namespace Errordite.Services.Serialisers
{
    public interface IMessageSerialiser
    {
        MessageBase Deserialise(string messageBody);
    }

    public class ReceiveErrorMessageSerialiser : IMessageSerialiser
    {
        public MessageBase Deserialise(string messageBody)
        {
            return JsonConvert.DeserializeObject<ErrorReceivedMessage>(messageBody);
        }
    }
}
