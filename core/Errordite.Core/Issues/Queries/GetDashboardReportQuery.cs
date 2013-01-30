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

            var dateResults = Query<IssueDailyCount, IssueDailyCount_Search>()
                .Where(i => i.OrganisationId == Organisation.GetId(request.OrganisationId))
                .ConditionalWhere(i => i.ApplicationId == Organisation.GetId(request.ApplicationId), request.ApplicationId.IsNotNullOrEmpty)
                .Where(i => i.Date >= request.StartDate && i.Date <= request.EndDate)
                .OrderBy(i => i.Date)
                .ToList();

            if (dateResults.Any())
            {
                var range = Enumerable.Range(0, (request.EndDate - request.StartDate).Days + 1).ToList();
                data = new
                {
                    x = range.Select(index => FindIssueCount(dateResults, request.StartDate.AddDays(index)).Date.ToString("yyyy-MM-dd")),
                    y = range.Select(index => FindIssueCount(dateResults, request.StartDate.AddDays(index)).Count)
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

        private IssueDailyCount FindIssueCount(IEnumerable<IssueDailyCount> results, DateTime date)
        {
            var result = results.FirstOrDefault(r => r.Date == date);

            if (result == null)
            {
                return new IssueDailyCount
                {
                    Count = 0,
                    Date = date
                };
            }

            return result;
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
