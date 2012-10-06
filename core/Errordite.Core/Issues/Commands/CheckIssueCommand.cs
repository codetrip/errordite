using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Raven.Client.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Commands
{
    /// <summary>
    /// Sets stuff about an issue based on certain rules.
    /// Specifically, it
    /// 1. sets the error count
    /// 2. deletes it if it is Unacknowledged and has no errors
    /// </summary>
    public class CheckIssueCommand : SessionAccessBase, ICheckIssueCommand
    {
        public CheckIssueResponse Invoke(CheckIssueRequest request)
        {
            Trace("Starting");

            var issue = Session.Raven.Load<Issue>(request.IssueId);

            if (issue == null)
            {
                Trace("Issue {0} not found", request.IssueId);
                return new CheckIssueResponse();
            }

            RavenQueryStatistics errorQueryStats;
            Query<Error, Errors_Search>()
                .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                .Statistics(out errorQueryStats)
                .Where(e => e.IssueId == request.IssueId)
                .Take(0)
                .ToList();

            issue.ErrorCount = errorQueryStats.TotalResults;

            Trace("Issue {0} error count {1}", issue.FriendlyId, issue.ErrorCount);

            if (issue.Status == IssueStatus.Unacknowledged && issue.ErrorCount == 0)
            {
                Trace("Deleting Unacknowledged issue as it has no errors");
                Delete(issue);
            }

            return new CheckIssueResponse();
        }
    }

    public interface ICheckIssueCommand : ICommand<CheckIssueRequest, CheckIssueResponse>
    { }

    public class CheckIssueResponse
    { }

    public class CheckIssueRequest
    {
        public string IssueId { get; set; }
    }
}
