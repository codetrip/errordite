using Errordite.Core.Messaging;

namespace Errordite.Services.Consumers
{
    public interface IErrorditeConsumer<in T> where T : MessageBase
    {
        void Consume(T message);
    }
}