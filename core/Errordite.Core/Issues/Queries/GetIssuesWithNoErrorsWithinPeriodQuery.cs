using System;
using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Raven.Client.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Queries
{
    public class GetIssuesWithNoErrorsWithinPeriodWithNoErrorsWithinPeriodQuery : SessionAccessBase, IGetIssuesWithNoErrorsWithinPeriodQuery
    {
        public GetIssuesWithNoErrorsWithinPeriodResponse Invoke(GetIssuesWithNoErrorsWithinPeriodRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;

            var retrievedEntities = Session.Raven.Query<IssueDocument, Issues_Search>().Statistics(out stats)
                .Where(i => i.ApplicationId == Application.GetId(request.ApplicationId))
                .Where(i => i.OrganisationId == Organisation.GetId(request.OrganisationId))
                .Where(i => i.LastErrorUtc < request.PurgeDate)
                .Skip((request.Paging.PageNumber - 1) * request.Paging.PageSize)
                .Take(request.Paging.PageSize)
                .As<Issue>()
                .ToList();

            var page = new Page<Issue>(retrievedEntities, new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            return new GetIssuesWithNoErrorsWithinPeriodResponse
            {
                Issues = page
            };
        }
    }

    public interface IGetIssuesWithNoErrorsWithinPeriodQuery : IQuery<GetIssuesWithNoErrorsWithinPeriodRequest, GetIssuesWithNoErrorsWithinPeriodResponse>
    { }

    public class GetIssuesWithNoErrorsWithinPeriodResponse
    {
        public Page<Issue> Issues { get; set; }
    }

    public class GetIssuesWithNoErrorsWithinPeriodRequest
    {
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }
        public DateTime PurgeDate { get; set; }
        public PageRequestWithSort Paging { get; set; }
    }
}
