﻿using Errordite.Core.Configuration;
using HipChat;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class NewIssueReceivedEmailInfo : IssueEmailInfoBase
    {
        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return string.Format(
                @"<b>{0}:</b> new issue <a href=""{1}/issue/{2}""  target=""_blank"">{2}: {3}</a>",
                ApplicationName, configuration.SiteBaseUrl, IssueId, IssueName
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
