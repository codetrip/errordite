using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues.Commands;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Errors.Queries
{
    public class FindErrorsMatchingRulesQuery : SessionAccessBase, IGetErrorsThatDoNotMatchNewRulesQuery
    {
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;

        public FindErrorsMatchingRulesQuery(IGetApplicationErrorsQuery getApplicationErrorsQuery)
        {
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
        }

        public FindErrorsMatchingRulesResponse Invoke(FindErrorsMatchingRulesRequest request)
        {
            Trace("Starting...");

            List<Error> matches;
            List<Error> nonMatches;
            GetMatches(request.IssueWithModifiedRules, request.IssueWithOldRules, out matches, out nonMatches);

            Trace("...Complete");

            return new FindErrorsMatchingRulesResponse
            {
                Status = AdjustRulesStatus.Ok,
                NonMatches = nonMatches,
                Matches = matches,
            };
        }

        private List<Error> GetMatches(Issue issueWithModifiedRules, Issue issueWithOldRules, out List<Error> matches, out List<Error> nonMatches)
        {
            nonMatches = new List<Error>();
            matches = new List<Error>();
            var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
            {
                ApplicationId = issueWithModifiedRules.ApplicationId,
                OrganisationId = issueWithModifiedRules.OrganisationId,
                Paging = new PageRequestWithSort(1, int.MaxValue, CoreConstants.SortFields.TimestampUtc, false),
                IssueId = issueWithModifiedRules.Id
            }).Errors;
            
            //find any errors which do not match the new rules and move them to our new temporary issue
            foreach (var error in errors.Items)
            {
                if (issueWithModifiedRules.Rules.All(rule => rule.IsMatch(error)))
                    matches.Add(error);
                else
                    nonMatches.Add(error);

                UpdateIssue(issueWithOldRules, error);
            } 

            return nonMatches;
        }

        private void UpdateIssue(Issue issue, Error error)
        {
            if (error.IssueId == issue.Id)
                return;

            //increment the new issue count
            issue.ErrorCount++;

            //attempt to correct the last error utc as this is lost when we create a new issue by adjusting rules of another issue
            if (issue.LastErrorUtc < error.TimestampUtc)
                issue.LastErrorUtc = error.TimestampUtc;
        }
    }

    public interface IGetErrorsThatDoNotMatchNewRulesQuery : ICommand<FindErrorsMatchingRulesRequest, FindErrorsMatchingRulesResponse>
    { }

    public class FindErrorsMatchingRulesResponse
    {
        public AdjustRulesStatus Status { get; set; }
        public List<Error> NonMatches { get; set; }
        public List<Error> Matches { get; set; }
    }

    public class FindErrorsMatchingRulesRequest
    {
        public Issue IssueWithOldRules { get; set; }
        public Issue IssueWithModifiedRules { get; set; }
    }
}
