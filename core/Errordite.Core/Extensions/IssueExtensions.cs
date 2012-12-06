using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.EmailInfo;

namespace Errordite.Core.Extensions  
{
    public static class IssueExtensions
    {
        public static IssueEmailInfoBase ToEmailInfo(this Issue issue, NotificationType notificationType, Error instance, Application application)
        {
            var emailInfo =
                notificationType == NotificationType.NotifyOnNewClassCreated
                    ? (IssueEmailInfoBase)new NewIssueReceivedEmailInfo()
                    : new NewInstanceOfSolvedIssueEmailInfo();

            emailInfo.ApplicationName = application.Name;
            emailInfo.ExceptionMessage = instance.ExceptionInfo.Message;
            emailInfo.Type = instance.ExceptionInfo.Type;
            emailInfo.Method = instance.ExceptionInfo.MethodName;
            emailInfo.IssueId = issue.FriendlyId;
            emailInfo.IssueName = issue.Name;
            //emailInfo.Subject = Resources.Notifications.ResourceManager.GetString(notificationType.ToString() + "_Subject");
            return emailInfo;
        }
    }
}
