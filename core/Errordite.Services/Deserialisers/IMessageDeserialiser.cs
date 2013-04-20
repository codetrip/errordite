using Errordite.Core.Messaging;

namespace Errordite.Services.Deserialisers
{
    public interface IMessageDeserialiser
    {
        MessageEnvelope Deserialise(string message);
    }
}
