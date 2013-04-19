
using System.Collections.Generic;
using Errordite.Core.Extensions;
using Errordite.Core.Configuration;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class NewIssueReceivedEmailInfo : IssueEmailInfoBase, IAlertInfo
    {
        IEnumerable<string> IAlertInfo.ToUserIds { get; set; }

        string[] IAlertInfo.Replacements
        {
            get { return new[] { ApplicationName, Type.Substring(Type.LastIndexOf('.') + 1), IssueId }; }
        }

        string IAlertInfo.MessageTemplate
        {
            get { return "{0} application ({1}). <a href='/issue/{2}'>View issue</a>."; }
        }

        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return Resources.Notifications.SimpleMessage_NewIssue.FormatWith(
                IssueName,
                configuration.SiteBaseUrl,
                IssueId);
        }
    }
}
