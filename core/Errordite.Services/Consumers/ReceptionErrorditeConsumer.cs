namespace Errordite.Services.Consumers
{
    public class ReceptionErrorditeConsumer : IErrorditeConsumer
    {
        public void Consume<T>(T message)
        {
            throw new System.NotImplementedException();
        }
    }
}