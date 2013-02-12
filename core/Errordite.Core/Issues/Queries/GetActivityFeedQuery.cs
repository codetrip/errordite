using System;
using System.Linq;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Raven.Client;

namespace Errordite.Core.Issues.Queries
{
    public class GetActivityFeedQuery : SessionAccessBase, IGetActivityFeedQuery
    {
        public GetActivityFeedResponse Invoke(GetActivityFeedRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;

            var history = Query<HistoryDocument, History_Search>().Statistics(out stats)
                .Skip((request.Paging.PageNumber - 1) * request.Paging.PageSize)
                .Take(request.Paging.PageSize)
                .OrderByDescending(e => e.DateAddedUtc);

            var page = new Page<IssueHistory>(history.As<IssueHistory>().ToList(), new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            return new GetActivityFeedResponse
            {
                Feed = page
            };
        }
    }

    public interface IGetActivityFeedQuery : IQuery<GetActivityFeedRequest, GetActivityFeedResponse>
    { }

    public class GetActivityFeedResponse
    {
        public Page<IssueHistory> Feed { get; set; }
    }

    public class GetActivityFeedRequest : OrganisationRequestBase
    {
        public PageRequestWithSort Paging { get; set; }
    }
}
