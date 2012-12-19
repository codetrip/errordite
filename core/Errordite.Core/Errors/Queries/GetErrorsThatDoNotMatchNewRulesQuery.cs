using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Session;

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

            var matches = new List<Error>();
            var nonMatches = new List<Error>();

            var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
            {
                ApplicationId = request.IssueWithModifiedRules.ApplicationId,
                OrganisationId = request.IssueWithModifiedRules.OrganisationId,
                Paging = new PageRequestWithSort(1, int.MaxValue),
                IssueId = request.IssueWithModifiedRules.Id
            }).Errors;

            //find any errors which do not match the new rules and move them to our new temporary issue
            foreach (var error in errors.Items)
            {
                if (request.IssueWithModifiedRules.Rules.All(rule => rule.IsMatch(error)))
                    matches.Add(error);
                else
                    nonMatches.Add(error);
            } 

            Trace("...Complete");

            return new GetErrorsThatDoNotMatchNewRulesResponse
            {
                Status = AdjustRulesStatus.Ok,
                NonMatches = nonMatches,
                Matches = matches,
            };
        }
    }

    public interface IGetErrorsThatDoNotMatchNewRulesQuery : ICommand<GetErrorsThatDoNotMatchNewRulesRequest, GetErrorsThatDoNotMatchNewRulesResponse>
    { }

    public class GetErrorsThatDoNotMatchNewRulesResponse
    {
        public AdjustRulesStatus Status { get; set; }
        public List<Error> NonMatches { get; set; }
        public List<Error> Matches { get; set; }
    }

    public class GetErrorsThatDoNotMatchNewRulesRequest
    {
        public Issue IssueWithOldRules { get; set; }
        public Issue IssueWithModifiedRules { get; set; }
    }
}
