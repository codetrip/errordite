
namespace Errordite.Services.Processors
{
    public interface IQueueProcessor
    {
        void Start(string organisationId = null);
        void Stop();
        string OrganisationId { get; }
    }
}