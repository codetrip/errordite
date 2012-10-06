
using CodeTrip.Core.Extensions;
using Errordite.Core.Notifications.Parsing;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class IssueErrorCountExceededEmailInfo : EmailInfoBase
    {
        public string IssueName { get; set; }
        [FriendlyId]
        public string IssueId { get; set; }
        public int ErrorCount { get; set; }

        public override string ConvertToSimpleMessage(Configuration.ErrorditeConfiguration configuration)
        {
            return Resources.Notifications.SimpleMessage_ErrorThresholdReached.FormatWith(IssueName, configuration.SiteBaseUrl, IssueId);
        }
    }
}
