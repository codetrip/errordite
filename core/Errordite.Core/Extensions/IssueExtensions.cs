using Errordite.Core.Exceptions;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;

namespace Errordite.Core.Extensions  
{
    public static class IssueExtensions
    {
        public static IssueEmailInfoBase ToEmailInfo(this Issue issue, NotificationType notificationType, Error instance, Application application)
        {
            IssueEmailInfoBase emailInfo;

            switch (notificationType)
            {
                case NotificationType.NotifyOnNewIssueCreated:
                    emailInfo = new NewIssueReceivedEmailInfo();
                    break;
                case NotificationType.NotifyOnNewInstanceOfSolvedIssue:
                    emailInfo = new NewInstanceOfSolvedIssueEmailInfo();
                    break;
                case NotificationType.AlwaysNotifyOnInstanceOfIssue:
                    emailInfo = new InstanceOfAlwaysNotifyIssueEmailInfo();
                    break;
                default:
                    throw new ErrorditeUnexpectedValueException("NotificationType", notificationType.ToString());
            }

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
