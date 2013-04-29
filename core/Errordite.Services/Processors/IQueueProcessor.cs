
namespace Errordite.Services.Processors
{
    public interface IQueueProcessor
    {
        void Start(string organisationId, string ravenInstanceId);
        void Stop();
        string OrganisationFriendlyId { get; }
        void PollNow();
    }
}