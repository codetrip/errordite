using System;
using System.Collections.Generic;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Session;

namespace Errordite.Core.Receive.Commands
{
    public class AttachToExistingIssueCommand : SessionAccessBase, IAttachToExistingIssueCommand
    {
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IMakeExceededErrorsUnloggedCommand _makeExceededErrorsUnloggedCommand;

        public AttachToExistingIssueCommand(ISendNotificationCommand sendNotificationCommand,
            ErrorditeConfiguration configuration, 
            IMakeExceededErrorsUnloggedCommand makeExceededErrorsUnloggedCommand)
        {
            _sendNotificationCommand = sendNotificationCommand;
            _configuration = configuration;
            _makeExceededErrorsUnloggedCommand = makeExceededErrorsUnloggedCommand;
        }

		public AttachToExistingIssueResponse Invoke(AttachToExistingIssueRequest request)
		{
			Trace("Attempting to load issue with Id {0} from database {1}", request.IssueId, Session.OrganisationDatabaseName);

			var issue = Load<Issue>(request.IssueId);

            issue.ErrorCount++;

			if (request.Error.TimestampUtc > issue.LastErrorUtc)
				issue.LastErrorUtc = request.Error.TimestampUtc;

	        var issueDailyCount = Load<IssueDailyCount>("IssueDailyCount/{0}-{1}".FormatWith(issue.FriendlyId, request.Error.TimestampUtc.ToString("yyyy-MM-dd")));

			if (issueDailyCount == null)
			{
				issueDailyCount = new IssueDailyCount
				{
					Id = "IssueDailyCount/{0}-{1}".FormatWith(issue.FriendlyId, request.Error.TimestampUtc.ToString("yyyy-MM-dd")),
					IssueId = issue.Id,
					Count = 1,
                    Date = request.Error.TimestampUtc.Date,
                    ApplicationId = issue.ApplicationId
				};

				Store(issueDailyCount);
			}
			else
			{
				issueDailyCount.Count++;
			}

	        var issueHourlyCount = Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(issue.FriendlyId));

            if (issueHourlyCount == null)
            {
                issueHourlyCount = new IssueHourlyCount
                {
                    IssueId = issue.Id,
					Id = "IssueHourlyCount/{0}".FormatWith(issue.FriendlyId),
					ApplicationId = issue.ApplicationId
                };

                issueHourlyCount.Initialise();
                issueHourlyCount.IncrementHourlyCount(issue.CreatedOnUtc);
                Store(issueHourlyCount);
            }
            else
            {
                issueHourlyCount.IncrementHourlyCount(request.Error.TimestampUtc);
            }

			SetLimitStatus(issue);

			Trace("Assigning issue Id to error with Id:={0}, Existing Error IssueId:={1}, New IssueId:={2}", request.Error.Id, request.Error.IssueId, issue.Id);
			request.Error.IssueId = issue.Id;

            //only store the error is it is a new error, not the result of reprocessing
            if (request.Error.Id.IsNullOrEmpty())
            {
                Trace("It's a new error, so Store it");
                Store(request.Error);

                //if the matching issue is solved, send an email and set it back to Acknowledged
				if (issue.Status == IssueStatus.Solved)
				{
				    issue.LastNotified = DateTime.UtcNow;

				    issue.Status = IssueStatus.Acknowledged;
				    SendNotification(issue, request.Application,
				                     NotificationType.NotifyOnNewInstanceOfSolvedIssue
				                     , request.Error, request.Organisation);
				} else if ((issue.NotifyFrequency ?? "0") != "0" && issue.LastNotified.GetValueOrDefault() < DateTime.UtcNow - new Duration(issue.NotifyFrequency))
                {
                    issue.LastNotified = DateTime.UtcNow;
                    SendNotification(issue, request.Application, NotificationType.AlwaysNotifyOnInstanceOfIssue,
                                     request.Error, request.Organisation, new Duration(issue.NotifyFrequency));


                }
            }

            if (issue.LimitStatus == ErrorLimitStatus.Exceeded)
            {
                _makeExceededErrorsUnloggedCommand.Invoke(new MakeExceededErrorsUnloggedRequest { IssueId = issue.Id });
            }

			return new AttachToExistingIssueResponse
			{
				Issue = issue
			};
        }

        private void SetLimitStatus(Issue issue)
        {
            if (issue.ErrorCount >= _configuration.IssueErrorLimit)
            {
				issue.LimitStatus = ErrorLimitStatus.Exceeded;
            }
        }

        private void SendNotification(Issue issue, Application application, NotificationType notificationType, Error instance, Organisation organisation, Duration duration = null)
        {
            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                OrganisationId = application.OrganisationId,
                Groups = application.NotificationGroups ?? new List<string>(),
                EmailInfo = issue.ToEmailInfo(notificationType, instance, application, duration),
                Application = application,
                Organisation = organisation
            });
        }  
    }

    public interface IAttachToExistingIssueCommand : ICommand<AttachToExistingIssueRequest, AttachToExistingIssueResponse>
    { }

    public class AttachToExistingIssueRequest
    {
        public Error Error { get; set; }
        public Application Application { get; set; }
        public Organisation Organisation { get; set; }
		public string IssueId { get; set; }
    }

    public class AttachToExistingIssueResponse
    {
        public Issue Issue { get; set; }
    }
}