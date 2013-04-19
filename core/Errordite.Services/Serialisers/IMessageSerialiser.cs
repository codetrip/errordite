
using Errordite.Core.Messages;
using Errordite.Services.Configuration;
using Errordite.Services.Entities;
using Newtonsoft.Json;

namespace Errordite.Services.Serialisers
{
    public interface IMessageSerialiser
    {
        MessageEnvelope Deserialise(string messageBody);
        ServiceInstance ForService { get; }
    }

    public class ReceiveErrorMessageSerialiser : IMessageSerialiser
    {
        public MessageEnvelope Deserialise(string messageBody)
        {
            var message = JsonConvert.DeserializeObject<ErrorReceivedMessage>(messageBody);
            return new MessageEnvelope
            {
                Message = message
            };
        }

        public ServiceInstance ForService
        {
            get { return ServiceInstance.Reception; }
        }
    }
}
