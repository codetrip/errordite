using Errordite.Core.Configuration;
using HipChat;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class PeriodicIssueNotificationEmailInfo : IssueEmailInfoBase
    {
        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return string.Format(@"<b>{0}:</b> issue <a href=""{1}/issue/{2}""  target=""_blank"">{2}: {3}</a> has occurred. Notification Frequency: {4}",
				ApplicationName, 
				configuration.SiteBaseUrl, 
				IssueId, 
				IssueName, 
				NotificationFrequency);
        }

		public override string ConvertToNonHtmlMessage(ErrorditeConfiguration configuration)
		{
			return string.Format(@"{0}: instance of issue '{3}' has occurred. Notification Frequency: {4}, view the issue here... {1}/issue/{2}",
				ApplicationName, 
				configuration.SiteBaseUrl, 
				IssueId, 
				IssueName, 
				NotificationFrequency);
		}

        public string NotificationFrequency { get; set; }

        public override HipChatClient.BackgroundColor? HipChatColour
        {
            get
            {
                return HipChatClient.BackgroundColor.purple;
            }
        }
    }
}
