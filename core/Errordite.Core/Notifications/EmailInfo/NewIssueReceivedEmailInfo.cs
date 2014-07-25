using Errordite.Core.Configuration;
using Errordite.Core.Domain;
using HipChat;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class NewIssueReceivedEmailInfo : IssueEmailInfoBase
    {
        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return string.Format(@"<b>{0}:</b> new issue <a href=""{1}""  target=""_blank"">{2}: {3}</a>",
                ApplicationName, 
				IssueUrl(configuration),
				IssueId, 
				IssueName);
        }

		public override string ConvertToNonHtmlMessage(ErrorditeConfiguration configuration)
		{
			return string.Format(@"{0}: new issue created named '{1}', view the issue here... {2}", 
				ApplicationName, 
				IssueName, 
                IssueUrl(configuration)
				);
		}

        public override HipChat.HipChatClient.BackgroundColor? HipChatColour
        {
            get
            {
                return HipChatClient.BackgroundColor.red;
            }
        }
    }
}
