using System;
using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Raven.Client.Linq;

namespace Errordite.Core.Issues.Queries
{
    public class GetApplicationIssuesQuery : SessionAccessBase, IGetApplicationIssuesQuery
    {
        public GetApplicationIssuesResponse Invoke(GetApplicationIssuesRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;

            var query = Session.Raven.Query<IssueDocument, Issues_Search>().Statistics(out stats)
                .Where(i => i.OrganisationId == Organisation.GetId(request.OrganisationId));

            if (request.ApplicationId.IsNotNullOrEmpty())
            {
                query = query.Where(i => i.ApplicationId == Application.GetId(request.ApplicationId));
            }

            if (request.StartDate.HasValue)
            {
                //TODO: I think this should be from the First Error (we are not currently recording this...)
                query = query.Where(i => i.LastErrorUtc >= TimeZoneInfo.ConvertTimeBySystemTimeZoneId(request.StartDate.Value, request.UserTimezoneId, "UTC"));
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(i => i.LastErrorUtc < TimeZoneInfo.ConvertTimeBySystemTimeZoneId(request.EndDate.Value.RangeEnd(), request.UserTimezoneId, "UTC"));
            }

            if (!request.Name.IsNullOrEmpty())
            {
                query = query.Where(i => i.Name == request.Name);
            }

            if (!request.AssignedTo.IsNullOrEmpty())
            {
                query = query.Where(i => i.UserId == User.GetId(request.AssignedTo));
            }

            if (request.Status != null && request.Status.Length > 0)
            {
                query = query.Where(i => i.Status.In(request.Status));
            }

            var issues = query
                .Skip((request.Paging.PageNumber - 1)*request.Paging.PageSize)
                .Take(request.Paging.PageSize);

            switch(request.Paging.Sort)
            {
                case "LastErrorUtc":
                    issues = request.Paging.SortDescending ? issues.OrderByDescending(e => e.LastErrorUtc) : issues.OrderBy(e => e.LastErrorUtc);
                    break;
                case "ErrorCount":
                    issues = request.Paging.SortDescending ? issues.OrderByDescending(e => e.ErrorCount) : issues.OrderBy(e => e.ErrorCount);
                    break;
                case "FriendlyId":
                    issues = request.Paging.SortDescending ? issues.OrderByDescending(e => e.FriendlyId) : issues.OrderBy(e => e.FriendlyId);
                    break;
            }

            var page = new Page<Issue>(issues.As<Issue>().ToList(), new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            return new GetApplicationIssuesResponse
            {
                Issues = page
            };
        }
    }

    public interface IGetApplicationIssuesQuery : IQuery<GetApplicationIssuesRequest, GetApplicationIssuesResponse>
    { }

    public class GetApplicationIssuesResponse
    {
        public Page<Issue> Issues { get; set; }
    }

    public class GetApplicationIssuesRequest
    {
        public string ApplicationId { get; set; }
        public string OrganisationId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AssignedTo { get; set; }
        public string Name { get; set; }
        public string[] Status { get; set; }
        public PageRequestWithSort Paging { get; set; }
        public string UserTimezoneId { get; set; }
    }
}
