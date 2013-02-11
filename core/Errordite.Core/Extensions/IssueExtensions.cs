using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;

namespace Errordite.Core.Extensions  
{
    public static class IssueExtensions
    {
        public static IssueEmailInfoBase ToEmailInfo(this Issue issue, NotificationType notificationType, Error instance,
                                                     Application application)
        {
            var emailInfo =
                notificationType == NotificationType.NotifyOnNewClassCreated
                    ? (IssueEmailInfoBase) new NewIssueReceivedEmailInfo()
                    : new NewInstanceOfSolvedIssueEmailInfo();

            emailInfo.ApplicationName = application.Name;
            emailInfo.ExceptionMessage = instance.ExceptionInfos.First().Message;
            emailInfo.Type = instance.ExceptionInfos.First().Type;
            emailInfo.Method = instance.ExceptionInfos.First().MethodName;
            emailInfo.IssueId = issue.FriendlyId;
            emailInfo.IssueName = issue.Name;
            return emailInfo;
        }
    }
}
