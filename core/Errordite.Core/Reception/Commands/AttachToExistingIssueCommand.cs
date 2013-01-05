using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Extensions;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Session;

namespace Errordite.Core.Reception.Commands
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

            //if the matching issue is solved, send an email and set it back to Acknowledged
            if (issue.Status == IssueStatus.Solved || (issue.AlwaysNotify && issue.LastErrorUtc < DateTime.UtcNow.AddHours(-12)))
            {
				MaybeSendNotification(issue, request.Application, NotificationType.NotifyOnNewInstanceOfSolvedClass, request.Error);

                if(issue.Status == IssueStatus.Solved)
                    issue.Status = IssueStatus.Acknowledged;
            }

            issue.ErrorCount++;

			if (request.Error.TimestampUtc > issue.LastErrorUtc)
				issue.LastErrorUtc = request.Error.TimestampUtc;

	        var issueDailyCount = Load<IssueDailyCount>("IssueDailyCount/{0}-{1}".FormatWith(issue.FriendlyId, issue.CreatedOnUtc.ToString("yyyy-MM-dd")));

			if (issueDailyCount == null)
			{
				issueDailyCount = new IssueDailyCount
				{
					Id = "IssueDailyCount/{0}-{1}".FormatWith(issue.FriendlyId, request.Error.TimestampUtc.ToString("yyyy-MM-dd")),
					IssueId = issue.Id,
					Count = 1,
                    Date = request.Error.TimestampUtc.Date,
                    CreatedOnUtc = DateTime.UtcNow
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
                    Id = "IssueHourlyCount/{0}".FormatWith(issue.FriendlyId)
                };

                issueHourlyCount.Initialise();
                issueHourlyCount.IncrementHourlyCount(issue.CreatedOnUtc);
                Store(issueHourlyCount);
            }
            else
            {
                issueHourlyCount.IncrementHourlyCount(request.Error.TimestampUtc);
            }

			SetLimitStatus(request.Application, issue);

			Trace("Assigning issue Id to error with Id:={0}, Existing Error IssueId:={1}, New IssueId:={2}", request.Error.Id, request.Error.IssueId, issue.Id);
			request.Error.IssueId = issue.Id;

            //only store the error is it is a new error, not the result of reprocessing
			if (request.Error.Id.IsNullOrEmpty())
            {
                Trace("It's a new error, so Store it");
				Store(request.Error);
            }

			if (issue.RulesMatch(request.Error))
            {
                Trace("Error definitely matches issue we are about to assign it to");
            }
            else
            {
                throw new InvalidOperationException(
                    "ERROR DOES NOT MATCH RULES Error with Id:={0} does not match issue {1} which it has been assigned to, applicationID:={2}"
						.FormatWith(request.Error.Id, issue.Id, issue.ApplicationId));
            }

            if (issue.LimitStatus == ErrorLimitStatus.Exceeded)
            {
                _makeExceededErrorsUnloggedCommand.Invoke(new MakeExceededErrorsUnloggedRequest {IssueId = issue.Id});
            }

			return new AttachToExistingIssueResponse
			{
				Issue = issue
			};
        }

        private void SetLimitStatus(Application application, Issue issue)
        {
            if (issue.ErrorCount >= _configuration.IssueErrorLimit)
            {
				issue.LimitStatus = ErrorLimitStatus.Exceeded;
            }
        }

        private void MaybeSendNotification(Issue issue, Application application, NotificationType notificationType, Error instance)
        {
            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                OrganisationId = application.OrganisationId,
                Groups = application.NotificationGroups ?? new List<string>(),
                EmailInfo = issue.ToEmailInfo(notificationType, instance, application),
                Application = application
            });
        }  
    }

    public interface IAttachToExistingIssueCommand : ICommand<AttachToExistingIssueRequest, AttachToExistingIssueResponse>
    { }

    public class AttachToExistingIssueRequest
    {
        public Error Error { get; set; }
		public Application Application { get; set; }
		public string IssueId { get; set; }
    }

    public class AttachToExistingIssueResponse
    {
        public Issue Issue { get; set; }
    }
}