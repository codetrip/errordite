using System;
using System.Collections.Generic;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using CodeTrip.Core.Extensions;
using Errordite.Core.Session;
using Raven.Abstractions.Data;

namespace Errordite.Core.Issues.Commands
{
    /// <summary>
    /// Component to resets the issues count documents after an operation which requires us to do so.
    /// Fistly deletes all DailyCount docs, then re-initializes the hourly count, finally loads all the errors
    /// for this issue and creates / updates the count docs and sets the issues error count and status
    /// </summary>
    public class ResetIssueErrorCountsCommand : SessionAccessBase, IResetIssueErrorCountsCommand
    {
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;

        public ResetIssueErrorCountsCommand(IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            ErrorditeConfiguration configuration)
        {
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _configuration = configuration;
        }

        public ResetIssueErrorCountsResponse Invoke(ResetIssueErrorCountsRequest request)
        {
            Trace("Starting...");
            Trace("Syncing issue with Id:={0}...", request.IssueId);

			TraceObject(request);

			var issue = Load<Issue>(Issue.GetId(request.IssueId));

			if (issue == null)
				return new ResetIssueErrorCountsResponse();

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

            //re-initialise the issue hourly counts
			var hourlyCount = Session.Raven.Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(issue.FriendlyId));

            if (hourlyCount == null)
            {
                hourlyCount = new IssueHourlyCount
                {
                    IssueId = issue.Id,
                    Id = "IssueHourlyCount/{0}".FormatWith(issue.FriendlyId)
                };

                Store(hourlyCount);
            }

            hourlyCount.Initialise();

            var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
            {
                ApplicationId = issue.ApplicationId,
                OrganisationId = issue.OrganisationId,
                IssueId = issue.Id,
                Paging = new PageRequestWithSort(1, int.MaxValue)
            }).Errors;

            var dailyCounts = new Dictionary<DateTime, IssueDailyCount>(); 
            foreach (var error in errors.Items)
            {
                hourlyCount.IncrementHourlyCount(error.TimestampUtc);

                if (dailyCounts.ContainsKey(error.TimestampUtc.Date))
                {
                    dailyCounts[error.TimestampUtc.Date].Count++;
                }
                else
                {
                    var dailyCount = new IssueDailyCount
                    {
                        Id = "IssueDailyCount/{0}-{1}".FormatWith(issue.FriendlyId, error.TimestampUtc.ToString("yyyy-MM-dd")),
                        IssueId = issue.Id,
                        Count = 1,
                        Date = error.TimestampUtc.Date,
                        ApplicationId = issue.ApplicationId
                    };

                    dailyCounts.Add(error.TimestampUtc.Date, dailyCount);

                    Trace("Creating IssueDailyCount, Id:={0}", dailyCount.Id);
                }

                if (issue.LastErrorUtc < error.TimestampUtc)
                    issue.LastErrorUtc = error.TimestampUtc;
            }

            //delete any daily issue count docs except the historical one
            new DeleteByIndexCommitAction(CoreConstants.IndexNames.IssueDailyCount, new IndexQuery { Query = "IssueId:{0} AND Historical:false" }, true).Execute(Session);

            //make sure the issue index is not stale
            new SynchroniseIndex<IssueDailyCount_Search>().Execute(Session);

            foreach (var dailyCount in dailyCounts)
            {
                Store(dailyCount.Value);
            }

            issue.ErrorCount = errors.PagingStatus.TotalItems;
			issue.LimitStatus = issue.ErrorCount >= _configuration.IssueErrorLimit ? ErrorLimitStatus.Exceeded : ErrorLimitStatus.Ok;

			return new ResetIssueErrorCountsResponse();
		}
	}

    public interface IResetIssueErrorCountsCommand : ICommand<ResetIssueErrorCountsRequest, ResetIssueErrorCountsResponse>
    { }

    public class ResetIssueErrorCountsResponse
    {}

    public class ResetIssueErrorCountsRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
    }
}
