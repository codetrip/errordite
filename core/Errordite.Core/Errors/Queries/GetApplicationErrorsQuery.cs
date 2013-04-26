using System;
using System.Linq;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Raven.Client;
using Raven.Client.Linq;
using Errordite.Core.Extensions;

namespace Errordite.Core.Errors.Queries
{
    public class GetApplicationErrorsQuery : SessionAccessBase, IGetApplicationErrorsQuery
    {
        public GetApplicationErrorsResponse Invoke(GetApplicationErrorsRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;
            
            var query = Session.Raven.Query<ErrorDocument, Errors_Search>()
                .Statistics(out stats);

            if (request.WaitForIndexStaleAtUtc.HasValue)
                query = query.Customize(c => c.WaitForNonStaleResultsAsOf(request.WaitForIndexStaleAtUtc.Value));

            if (!request.ApplicationId.IsNullOrEmpty())
            {
                query = query.Where(e => e.ApplicationId == Application.GetId(request.ApplicationId));
            }

            if (request.LastFriendlyId.HasValue)
            {
                query = query.Where(e => e.FriendlyId > request.LastFriendlyId);
            }

            if (!request.Query.IsNullOrEmpty())
            {
                query = query.Where(e => e.Query == request.Query);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(e => e.TimestampUtc >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(e => e.TimestampUtc < request.EndDate.Value.RangeEnd());
            }

            if (!request.IssueId.IsNullOrEmpty())
            {
                query = query.Where(e => e.IssueId == Issue.GetId(request.IssueId));
            }

            var errors = query
                .Skip((request.Paging.PageNumber - 1)*request.Paging.PageSize)
                .Take(request.Paging.PageSize);

            switch (request.Paging.Sort)
            {
                case "TimestampUtc":
                    errors = request.Paging.SortDescending ? errors.OrderByDescending(e => e.TimestampUtc) : errors.OrderBy(e => e.TimestampUtc);
                    break;
                case "FriendlyId":
                    errors = request.Paging.SortDescending ? errors.OrderByDescending(e => e.FriendlyId) : errors.OrderBy(e => e.FriendlyId);
                    break;
            }

            var page = new Page<Error>(errors.As<Error>().ToList(), new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            Trace("...Located {0} Items from page {1}, page size:={2}", stats.TotalResults, request.Paging.PageNumber, request.Paging.PageSize);

            return new GetApplicationErrorsResponse
            {
                Errors = page
            };
        }
    }

    public interface IGetApplicationErrorsQuery : IQuery<GetApplicationErrorsRequest, GetApplicationErrorsResponse>
    { }

    public class GetApplicationErrorsResponse
    {
        public Page<Error> Errors { get; set; }
    }

    public class GetApplicationErrorsRequest
    {
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }
        public string Query { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
		public string IssueId { get; set; }
        public PageRequestWithSort Paging { get; set; }
        public int? LastFriendlyId { get; set; }

        public DateTime? WaitForIndexStaleAtUtc { get; set; }
    }
}
