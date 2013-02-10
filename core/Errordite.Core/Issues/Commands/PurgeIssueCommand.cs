using System;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using CodeTrip.Core.Extensions;
using Errordite.Core.Session;

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

			Session.Raven.Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(issue.FriendlyId)).Initialise();

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

            issue.History.Add(new IssueHistory
                {
                    DateAddedUtc = DateTime.UtcNow,
                    UserId = request.CurrentUser.Id,
                    Type = HistoryItemType.ErrorsPurged,
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
