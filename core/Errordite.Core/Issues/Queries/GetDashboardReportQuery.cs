using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Extensions;

namespace Errordite.Core.Issues.Queries
{
    public class GetDashboardReportQuery : SessionAccessBase, IGetDashboardReportQuery
    {
        public GetDashboardReportResponse Invoke(GetDashboardReportRequest request)
        {
            Trace("Starting...");

            object data;

            var dateResults = Query<IssueDailyCount, OrganisationDailyCount_Search>()
                .ConditionalWhere(i => i.ApplicationId == Organisation.GetId(request.ApplicationId), request.ApplicationId.IsNotNullOrEmpty)
                .Where(i => i.Date >= request.StartDate.Date && i.Date <= request.EndDate.Date)
                .OrderBy(i => i.Date)
                .ToList();

            foreach (var result in dateResults)
            {
                Trace("DASHBOARD: Date:{0}, IssueId:={1}, Count:={2}", result.Date, result.IssueId, result.Count);
            }

            if (dateResults.Any())
            {
                var range = Enumerable.Range(0, (request.EndDate - request.StartDate).Days + 1).ToList();
                data = new
                {
                    x = range.Select(index => request.StartDate.AddDays(index).Date.ToString("yyyy-MM-dd")),
                    y = range.Select(index => FindIssueCount(dateResults, request.StartDate.AddDays(index)))
                };
            }
            else
            {
                var range = Enumerable.Range(0, (request.EndDate - request.StartDate).Days + 1).ToList();
                data = new
                {
                    x = range.Select(d => request.StartDate.AddDays(d).ToString("yyyy-MM-dd")),
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

    public class GetDashboardReportRequest : OrganisationRequestBase
    {
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
