
namespace Errordite.Services.Consumers
{
    public interface IErrorditeConsumer
    {
        void Consume<T>(T message);
    }

    public class ReceiveErrorConsumer : IErrorditeConsumer
    {
        public void Consume<T>(T message)
        {
           
        }
    }
}