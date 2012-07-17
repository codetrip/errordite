using CodeTrip.Core.Extensions;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class IssueErrorCountWarningEmailInfo : EmailInfoBase
    {
        public string IssueName { get; set; }
        public string IssueId { get; set; }
        public int ErrorCount { get; set; }
        public int ErrorLimitCount { get; set; }

        public override string ConvertToSimpleMessage(Configuration.ErrorditeConfiguration configuration)
        {
            return Resources.Notifications.SimpleMessage_ErrorThresholdWarning.FormatWith(IssueName, configuration.SiteBaseUrl, IssueId);
        }
    }
}
