using Errordite.Core.Messages;

namespace Errordite.Services.Consumers
{
    public interface IErrorditeConsumer<in T> where T : MessageBase
    {
        void Consume(T message);
    }
}