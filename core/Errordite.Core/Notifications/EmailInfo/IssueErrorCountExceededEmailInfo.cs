
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class IssueErrorCountExceededEmailInfo : EmailInfoBase
    {
        public string IssueName { get; set; }
        public string IssueId { get; set; }
        public int ErrorCount { get; set; }

        public override string ConvertToSimpleMessage(Configuration.ErrorditeConfiguration configuration)
        {
            return Resources.Notifications.SimpleMessage_ErrorThresholdReached.FormatWith(IssueName, configuration.Endpoint, IssueId);
        }
    }
}
