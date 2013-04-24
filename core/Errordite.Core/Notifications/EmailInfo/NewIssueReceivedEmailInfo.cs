
using Errordite.Core.Extensions;
using Errordite.Core.Configuration;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class NewIssueReceivedEmailInfo : IssueEmailInfoBase
    {
        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return Resources.Notifications.SimpleMessage_NewIssue.FormatWith(
                IssueName,
                configuration.SiteBaseUrl,
                IssueId);
        }
    }
}
