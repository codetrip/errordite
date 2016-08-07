using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Raven.Client;

namespace Errordite.Core.Issues.Queries
{
    public class GetActivityLogQuery : SessionAccessBase, IGetActivityLogQuery
    {
        public GetActivityLogResponse Invoke(GetActivityLogRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;

            var history = Query<HistoryDocument, History>().Statistics(out stats)
                .ConditionalWhere(h => h.ApplicationId == Application.GetId(request.ApplicationId), request.ApplicationId.IsNotNullOrEmpty)
                .Skip((request.Paging.PageNumber - 1) * request.Paging.PageSize)
                .Take(request.Paging.PageSize)
                .OrderByDescending(e => e.DateAddedUtc);

            var page = new Page<IssueHistory>(history.As<IssueHistory>().ToList(), new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            return new GetActivityLogResponse
            {
                Log = page
            };
        }
    }

    public interface IGetActivityLogQuery : IQuery<GetActivityLogRequest, GetActivityLogResponse>
    { }

    public class GetActivityLogResponse
    {
        public Page<IssueHistory> Log { get; set; }
    }

    public class GetActivityLogRequest : OrganisationRequestBase
    {
        public PageRequestWithSort Paging { get; set; }
        public string ApplicationId { get; set; }
    }
}
