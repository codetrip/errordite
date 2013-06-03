
namespace Errordite.Services.Processors
{
    public interface IQueueProcessor
    {
        void Start(string organisationId, string ravenInstanceId);
		void Stop();
		void StopPolling();
        string OrganisationFriendlyId { get; }
        void PollNow();
    }
}