using System;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Extensions;
using Errordite.Core.Organisations;
using Errordite.Core.Extensions;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;

namespace Errordite.Core.Issues.Commands
{
    public class DeleteIssueErrorsCommand : SessionAccessBase, IDeleteIssueErrorsCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public DeleteIssueErrorsCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public DeleteIssueErrorsResponse Invoke(DeleteIssueErrorsRequest request)
        {
			Trace("Starting...");
			TraceObject(request);

			var issue = Load<Issue>(Issue.GetId(request.IssueId));

			if (issue == null)
				return new DeleteIssueErrorsResponse();

			_authorisationManager.Authorise(issue, request.CurrentUser);

			//delete the issue's errors
            Session.AddCommitAction(new DeleteAllErrorsCommitAction(issue.Id));
            Session.AddCommitAction(new DeleteAllDailyCountsCommitAction(issue.Id));

            var hourlyCount = Session.Raven.Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(issue.FriendlyId));

            if (hourlyCount == null)
            {
                hourlyCount = new IssueHourlyCount
                {
                    IssueId = issue.Id,
                    Id = "IssueHourlyCount/{0}".FormatWith(issue.FriendlyId)
                };
                hourlyCount.Initialise();
                Store(hourlyCount);
            }
            else
            {
                hourlyCount.Initialise();
            }

            //create or update the historical count of errors for this issue
            var historicalCount = Load<IssueDailyCount>("IssueDailyCount/{0}-Historical".FormatWith(issue.FriendlyId));

            if (historicalCount == null)
            {
                historicalCount = new IssueDailyCount
                {
                    IssueId = issue.Id,
                    ApplicationId = issue.ApplicationId,
                    Count = issue.ErrorCount,
                    Date = DateTime.MinValue.Date,
                    Historical = true,
                    Id = "IssueDailyCount/{0}-Historical".FormatWith(issue.FriendlyId)
                };

                Store(historicalCount);
            }
            else
            {
                historicalCount.Count += issue.ErrorCount;
            }

            Store(new IssueHistory
            {
                DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.Organisation.TimezoneId),
                UserId = request.CurrentUser.Id,
                Type = HistoryItemType.ErrorsPurged,
                IssueId = issue.Id,
                ApplicationId = issue.ApplicationId,
            });
            
            issue.ErrorCount = 0;
			issue.LimitStatus = ErrorLimitStatus.Ok;

			return new DeleteIssueErrorsResponse();
		}
	}

    public interface IDeleteIssueErrorsCommand : ICommand<DeleteIssueErrorsRequest, DeleteIssueErrorsResponse>
    { }

    public class DeleteIssueErrorsResponse
    {}

    public class DeleteIssueErrorsRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
    }
}
