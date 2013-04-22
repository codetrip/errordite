
namespace Errordite.Services.Queuing
{
    public interface IQueueProcessor
    {
        void Start(string organisationId = null);
        void Stop();
    }
}