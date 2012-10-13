using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Indexing;
using Raven.Client.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Queries
{
    public class GetIssueWithMatchingRulesQuery : SessionAccessBase, IGetIssueWithMatchingRulesQuery
    {
        public GetIssueWithMatchingRulesResponse Invoke(GetIssueWithMatchingRulesRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;

            var q = Session.Raven.Query<IssueDocument, Issues_Search>().Statistics(out stats)
                .Where(i => i.OrganisationId == request.IssueToMatch.OrganisationId 
                    && i.ApplicationId == request.IssueToMatch.ApplicationId
                    && i.RulesHash == request.IssueToMatch.RulesHash);

            if (request.IssueToMatch.Id != null)
                q = q.Where(i => i.Id != request.IssueToMatch.Id);

            var issue = q.As<Issue>().FirstOrDefault();

            return new GetIssueWithMatchingRulesResponse
            {
                Issue = issue,
                Count = stats.TotalResults
            };
        }
    }

    public interface IGetIssueWithMatchingRulesQuery : IQuery<GetIssueWithMatchingRulesRequest, GetIssueWithMatchingRulesResponse>
    { }

    public class GetIssueWithMatchingRulesResponse
    {
        public Issue Issue { get; set; }
        public int Count { get; set; }
    }

    public class GetIssueWithMatchingRulesRequest
    {
        public Issue IssueToMatch { get; set; }
    }
}
