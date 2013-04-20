
using CodeTrip.Core.Extensions;
using Errordite.Core.Configuration;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class InstanceOfAlwaysNotifyIssueEmailInfo : IssueEmailInfoBase
    {
        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return Resources.Notifications.SimpleMessage_SolvedIssueOccurance.FormatWith(
                IssueName,
                configuration.SiteBaseUrl,
                IssueId);
        }
    }
}
