using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues.Commands;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Errors.Queries
{
    public class GetErrorsThatDoNotMatchNewRulesQuery : SessionAccessBase, IGetErrorsThatDoNotMatchNewRulesQuery
    {
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;

        public GetErrorsThatDoNotMatchNewRulesQuery(IGetApplicationErrorsQuery getApplicationErrorsQuery)
        {
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
        }

        public GetErrorsThatDoNotMatchNewRulesResponse Invoke(GetErrorsThatDoNotMatchNewRulesRequest request)
        {
            Trace("Starting...");

            int matchCount;
            var nonMatches = GetNonMatches(request.IssueWithModifiedRules, request.IssueWithOldRules, out matchCount);

            Trace("...Complete");

            return new GetErrorsThatDoNotMatchNewRulesResponse
            {
                Status = AdjustRulesStatus.Ok,
                Errors = nonMatches,
                MatchCount = matchCount,
            };
        }

        private List<Error> GetNonMatches(Issue issueWithModifiedRules, Issue issueWithOldRules, out int matchCount)
        {
            var nonMatches = new List<Error>();
            var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
            {
                ApplicationId = issueWithModifiedRules.ApplicationId,
                OrganisationId = issueWithModifiedRules.OrganisationId,
                Paging = new PageRequestWithSort(1, int.MaxValue, CoreConstants.SortFields.TimestampUtc, false),
                IssueId = issueWithModifiedRules.Id
            }).Errors;
            
            //find any errors which do not match the new rules and move them to our new temporary issue
            foreach (var error in errors.Items.Where(errorInstance => !issueWithModifiedRules.Rules.All(rule => rule.IsMatch(errorInstance))))
            {
                nonMatches.Add(error);
                UpdateIssue(issueWithOldRules, error);
            } 
        
            matchCount = errors.Items.Count - nonMatches.Count;

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

    public interface IGetErrorsThatDoNotMatchNewRulesQuery : ICommand<GetErrorsThatDoNotMatchNewRulesRequest, GetErrorsThatDoNotMatchNewRulesResponse>
    { }

    public class GetErrorsThatDoNotMatchNewRulesResponse
    {
        public AdjustRulesStatus Status { get; set; }
        public List<Error> Errors { get; set; }
        public int MatchCount { get; set; }
    }

    public class GetErrorsThatDoNotMatchNewRulesRequest
    {
        public Issue IssueWithOldRules { get; set; }
        public Issue IssueWithModifiedRules { get; set; }
    }
}
