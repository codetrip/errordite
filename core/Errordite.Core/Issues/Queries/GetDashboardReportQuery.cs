using System;
using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;

namespace Errordite.Core.Issues.Queries
{   
    //GT: don't cache this - Raven returns a 304 if unchanged and we want to get updates asap
    public class GetDashboardReportQuery : SessionAccessBase, IGetDashboardReportQuery
    {
        public GetDashboardReportResponse Invoke(GetDashboardReportRequest request)
        {
            Trace("Starting...");

            var startDate = DateTime.UtcNow.Date.AddMonths(-1).Date;
            var endDate = DateTime.UtcNow.Date.AddHours(2);
            object data;

            var dateResults = Query<IssueDailyCount, OrganisationDailyCounts>()
                .ConditionalWhere(i => i.ApplicationId == Application.GetId(request.ApplicationId), request.ApplicationId.IsNotNullOrEmpty)
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .OrderBy(i => i.Date)
                .ToList();

            if (dateResults.Any())
            {
                var range = Enumerable.Range(0, (endDate - startDate).Days + 1).ToList();
                data = new
                {
					x = range.Select(index => startDate.AddDays(index).Date.ConvertToUnixTimestamp()),
                    y = range.Select(index => FindIssueCount(dateResults, startDate.AddDays(index)))
                };
            }
            else
            {
                var range = Enumerable.Range(0, (endDate - startDate).Days + 1).ToList();
                data = new
                {
					x = range.Select(d => startDate.AddDays(d).Date.ConvertToUnixTimestamp()),
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

    public class GetDashboardReportRequest 
    {
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }
    }
}
