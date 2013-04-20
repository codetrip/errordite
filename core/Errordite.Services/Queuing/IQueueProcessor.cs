namespace Errordite.Services.Queuing
{
    public interface IQueueProcessor
    {
        void Start();
        void Stop();
    }
}