using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Errordite.Core.Extensions;

namespace Errordite.Core.Issues.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetDashboardReportQuery : SessionAccessBase, IGetDashboardReportQuery
    {
        public GetDashboardReportResponse Invoke(GetDashboardReportRequest request)
        {
            Trace("Starting...");

            var startDate = DateTime.UtcNow.Date.AddDays(-7);
            var endDate = DateTime.UtcNow.Date.AddHours(2);
            object data;

            var dateResults = Query<IssueDailyCount, OrganisationDailyCount_Search>()
                .ConditionalWhere(i => i.ApplicationId == Application.GetId(request.ApplicationId), request.ApplicationId.IsNotNullOrEmpty)
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .OrderBy(i => i.Date)
                .ToList();

            if (dateResults.Any())
            {
                var range = Enumerable.Range(0, (endDate - startDate).Days + 1).ToList();
                data = new
                {
                    x = range.Select(index => startDate.AddDays(index).Date.ToString("yyyy-MM-dd")),
                    y = range.Select(index => FindIssueCount(dateResults, startDate.AddDays(index)))
                };
            }
            else
            {
                var range = Enumerable.Range(0, (endDate - startDate).Days + 1).ToList();
                data = new
                {
                    x = range.Select(d => startDate.AddDays(d).ToString("yyyy-MM-dd")),
                    y = range.Select(d => 0)
                };
            }

            return new GetDashboardReportResponse
            {
                Data = data
            };
        }

        private int FindIssueCount(IEnumerable<IssueDailyCount> results, DateTime date)
        {
            return results.Where(r => r.Date == date).Sum(r => r.Count);
        }
    }

    public interface IGetDashboardReportQuery : IQuery<GetDashboardReportRequest, GetDashboardReportResponse>
    { }

    public class GetDashboardReportResponse
    {
        public object Data { get; set; }
    }

    public class GetDashboardReportRequest : CacheableRequestBase<GetDashboardReportResponse>
    {
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }

        protected override string GetCacheKey()
        {
            return "dashboard-" + CacheKeys.Applications.Key(OrganisationId, ApplicationId);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Dashboard;
        }
    }
}
