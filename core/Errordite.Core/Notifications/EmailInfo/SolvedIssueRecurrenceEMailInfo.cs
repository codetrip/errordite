using Errordite.Core.Configuration;
using HipChat;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class SolvedIssueRecurrenceEMailInfo : IssueEmailInfoBase
    {
        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return string.Format(@"<b>{0}:</b> solved issue <a href=""{1}""  target=""_blank"">{2}: {3}</a> has recurred",
                ApplicationName, 
				IssueUrl(configuration),
				IssueId, 
				IssueName);
        }

		public override string ConvertToNonHtmlMessage(ErrorditeConfiguration configuration)
		{
			return string.Format(@"{0}: solved issue named '{1}' has recurred, view the issue here... {2}",
				ApplicationName,
                IssueName,
				IssueUrl(configuration)
				);
		}

        public override HipChatClient.BackgroundColor? HipChatColour
        {
            get
            {
                return HipChatClient.BackgroundColor.red;
            }
        }
    }
}
