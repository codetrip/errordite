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
using Raven.Abstractions.Linq;

namespace Errordite.Core.Issues.Commands
{
    /// <summary>
    /// Component to resync the issues count documents after an operation which requires us to do so.
    /// Fistly deletes all DailyCount docs, then re-initializes the hourly count, finally loads all the errors
    /// for this issue and creates / updates the count docs and sets the issues error count and status
    /// </summary>
    public class SyncIssueErrorCountsCommand : SessionAccessBase, ISyncIssueErrorCountsCommand
    {
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;

        public SyncIssueErrorCountsCommand(IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            ErrorditeConfiguration configuration)
        {
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _configuration = configuration;
        }

        public SyncIssueErrorCountsResponse Invoke(SyncIssueErrorCountsRequest request)
        {
			Trace("Starting...");
			TraceObject(request);

			var issue = Load<Issue>(Issue.GetId(request.IssueId));

			if (issue == null)
				return new SyncIssueErrorCountsResponse();

            //make sure the errors index is not stale
            new SynchroniseIndex<Errors_Search>().Execute(Session);

            string query = "(IssueId:{0} AND CreatedOnUtc:[* TO {1}])".FormatWith(issue.Id, DateTime.UtcNow.ToString(CoreConstants.QueryDateFormat));

            Trace("Deleting counts with query:={0}", query);

			//delete any daily issue count docs
            Session.AddCommitAction(new DeleteByIndexCommitAction(CoreConstants.IndexNames.IssueDailyCount, new IndexQuery { Query = query }, true));

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

            //update the last sync utc for this issue
            issue.LastSyncUtc = DateTime.UtcNow.AddMilliseconds(100);

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
                    dailyCounts.Add(error.TimestampUtc.Date, new IssueDailyCount
                    {
                        IssueId = issue.Id,
                        Count = 1,
                        Date = error.TimestampUtc.Date,
                        CreatedOnUtc = DateTime.UtcNow.AddMilliseconds(100)
                    });
                }

                if (issue.LastErrorUtc < error.TimestampUtc)
                    issue.LastErrorUtc = error.TimestampUtc;
            }

            foreach (var dailyCount in dailyCounts)
            {
                Store(dailyCount.Value);
            }

            Trace("Last sync:={0}", issue.LastSyncUtc);

            issue.ErrorCount = errors.PagingStatus.TotalItems;
			issue.LimitStatus = issue.ErrorCount >= _configuration.IssueErrorLimit ? ErrorLimitStatus.Exceeded : ErrorLimitStatus.Ok;

			return new SyncIssueErrorCountsResponse();
		}
	}

    public interface ISyncIssueErrorCountsCommand : ICommand<SyncIssueErrorCountsRequest, SyncIssueErrorCountsResponse>
    { }

    public class SyncIssueErrorCountsResponse
    {}

    public class SyncIssueErrorCountsRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
    }
}
