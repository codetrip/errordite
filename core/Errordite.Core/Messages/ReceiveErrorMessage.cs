
using Errordite.Core.Domain.Error;
using Errordite.Core.ServiceBus;

namespace Errordite.Core.Messages
{
    public class ReceiveErrorMessage : ErrorditeNServiceBusMessageBase
    {
        public Error Error { get; set; }
        public string ApplicationId { get; set; }
        public string OrganisationId { get; set; }
        public string Token { get; set; }
        public string ExistingIssueId { get; set; }

        public ReceiveErrorMessage()
        {
            DoNotAudit = true;
        }
    }
}
