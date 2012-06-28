
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
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
            get { return "{0} application ({1}). <a href='/issue/index/{2}'>View issue</a>."; }
        }

        public override string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return Resources.Notifications.SimpleMessage_NewIssue.FormatWith(
                IssueName,
                configuration.Endpoint,
                IssueId);
        }
    }
}
