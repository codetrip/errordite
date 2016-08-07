using System;
using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;

namespace Errordite.Core.Issues.Queries
{   
	public class GetMostRecurringIssuesForDateQuery : SessionAccessBase, IMostRecurringIssuesForDateQuery
    {
        public GetMostRecurringIssuesForDateResponse Invoke(GetMostRecurringIssuesForDateRequest request)
        {
            Trace("Starting...");

			var results = Query<IssueDailyCount, IssueDailyCounts>()
                .ConditionalWhere(i => i.ApplicationId == Application.GetId(request.ApplicationId), request.ApplicationId.IsNotNullOrEmpty)
                .Where(i => i.Date == request.Date.Date)
                .OrderByDescending(i => i.Count)
				.Take(10)
                .ToList();

	        var issues = Session.Raven.Load<Issue>(results.Select(i => i.IssueId));

	        return new GetMostRecurringIssuesForDateResponse
		    {
			    Data = results.Select(r => new
				{
					Id = r.IssueId.GetFriendlyId(),
					Name = GetIssueName(issues, r.IssueId),
					r.Count
				})
		    };
        }

		private string GetIssueName(IEnumerable<Issue> issues, string issueId)
		{
			var issue = issues.FirstOrDefault(i => i.Id == issueId);
			return issue == null ? string.Empty : issue.Name;
	    }
    }

    public interface IMostRecurringIssuesForDateQuery : IQuery<GetMostRecurringIssuesForDateRequest, GetMostRecurringIssuesForDateResponse>
    { }

    public class GetMostRecurringIssuesForDateResponse
    {
		public object Data { get; set; }
    }

    public class GetMostRecurringIssuesForDateRequest 
    {
		public string ApplicationId { get; set; }
		public DateTime Date { get; set; }
    }
}
