using System;
using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Issues;
using Errordite.Core.Matching;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Session;

namespace Errordite.Core.Receive.Commands
{
    public class AttachToNewIssueCommand : SessionAccessBase, IAttachToNewIssueCommand
    {
        private readonly IMatchRuleFactoryFactory _matchRuleFactoryFactory;
        private readonly IReceptionServiceIssueCache _receptionServiceIssueCache;
        private readonly ISendNotificationCommand _sendNotificationCommand;

        public AttachToNewIssueCommand(IMatchRuleFactoryFactory matchRuleFactoryFactory, 
            ISendNotificationCommand sendNotificationCommand, 
            IReceptionServiceIssueCache receptionServiceIssueCache)
        {
            _matchRuleFactoryFactory = matchRuleFactoryFactory;
            _sendNotificationCommand = sendNotificationCommand;
            _receptionServiceIssueCache = receptionServiceIssueCache;
        }

		public AttachToNewIssueResponse Invoke(AttachToNewIssueRequest request)
        {
            Trace("Starting...");

			//mark the error as unclassified
			var error = request.Error;
			var application = request.Application;
			var matchRuleFactory = _matchRuleFactoryFactory.Create(application.MatchRuleFactoryId);
			var rules = matchRuleFactory.Create(error).ToList();

			var issue = new Issue
			{
                Name = "{0} ({1})".FormatWith(error.ExceptionInfos.First().Type, error.TimestampUtc.ToString("yyyy.MM.ddTHH.mm.ss")),
				Rules = rules,
				ApplicationId = application.Id,
				CreatedOnUtc = error.TimestampUtc,
				LastModifiedUtc = error.TimestampUtc,
				UserId = application.DefaultUserId,
				ErrorCount = 1,
				LastErrorUtc = error.TimestampUtc,
				OrganisationId = application.OrganisationId,
				TestIssue = error.TestError
			};

			Store(issue);
            Store(new IssueHistory
            {
                DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(application.TimezoneId),
                Type = HistoryItemType.AutoCreated,
                ExceptionType = error.ExceptionInfos.First().Type,
                ExceptionMethod = error.ExceptionInfos.First().MethodName,
                ExceptionMachine = error.MachineName,
                SystemMessage = true,
                IssueId = issue.Id,
                ApplicationId = issue.ApplicationId,
            });

			var issueHourlyCount = new IssueHourlyCount
			{
				IssueId = issue.Id,
				Id = "IssueHourlyCount/{0}".FormatWith(issue.FriendlyId),
				ApplicationId = issue.ApplicationId
			};

			issueHourlyCount.Initialise();
			issueHourlyCount.IncrementHourlyCount(issue.CreatedOnUtc);
			Store(issueHourlyCount);

			var issueDailyCount = new IssueDailyCount
			{
				Id = "IssueDailyCount/{0}-{1}".FormatWith(issue.FriendlyId, issue.CreatedOnUtc.ToString("yyyy-MM-dd")),
				IssueId = issue.Id,
				Count = 1,
				Date = issue.CreatedOnUtc.Date,
                ApplicationId = issue.ApplicationId
			};

			Store(issueDailyCount);

			Trace("AttachTod issue: Id:={0}, Name:={1}", issue.Id, issue.Name);
			error.IssueId = issue.Id;
			SendNotification(issue, application, NotificationType.NotifyOnNewIssueCreated, error, request.Organisation);

			//tell the issue cache we have a new issue
			_receptionServiceIssueCache.Add(issue);

			Store(error);

            Trace("Complete");

            return new AttachToNewIssueResponse
            {
                Issue = issue
            };
        }

        private void SendNotification(Issue issue, Application application, NotificationType notificationType, Error instance, Organisation organisation)
        {
            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                OrganisationId = application.OrganisationId,
                Groups = application.NotificationGroups ?? new List<string>(),
                EmailInfo = issue.ToEmailInfo(notificationType, instance, application),
                Application = application,
                Organisation = organisation
            });
        }
    }

	public interface IAttachToNewIssueCommand : ICommand<AttachToNewIssueRequest, AttachToNewIssueResponse>
    { }

	public class AttachToNewIssueRequest
    {
        public Error Error { get; set; }
        public Application Application { get; set; }
        public Organisation Organisation { get; set; }
    }

	public class AttachToNewIssueResponse
    {
        public Issue Issue { get; set; }
    }
}