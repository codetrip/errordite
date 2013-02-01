using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Models.Dashboard;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using CodeTrip.Core.Extensions;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class DashboardController : ErrorditeController
    {
        private readonly IGetApplicationIssuesQuery _getApplicationIssuesQuery;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IGetOrganisationStatisticsQuery _getOrganisationStatisticsQuery;
        private readonly IGetDashboardReportQuery _getDashboardReportQuery;

        public DashboardController(IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery, 
            IGetApplicationIssuesQuery getApplicationIssuesQuery, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IGetDashboardReportQuery getDashboardReportQuery)
        {
            _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
            _getApplicationIssuesQuery = getApplicationIssuesQuery;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _getDashboardReportQuery = getDashboardReportQuery;
        }

        [ImportViewData]
        public ActionResult GetGraphData(string applicationId)
        {
            var data = _getDashboardReportQuery.Invoke(new GetDashboardReportRequest
            {
                ApplicationId = applicationId,
                OrganisationId = Core.AppContext.CurrentUser.OrganisationId
            }).Data;

            return new PlainJsonNetResult(data, true);
        }

        [ImportViewData]
        public ActionResult Index(string applicationId)
        {
            var viewModel = new DashboardViewModel();
            var applications = Core.GetApplications();
            
            if(applications.PagingStatus.TotalItems > 0)
            {
                viewModel.HasApplications = true;

                if (applications.Items.Count == 1)
                {
                    viewModel.SingleApplicationId = applications.Items[0].Id;
                    viewModel.SingleApplicationToken = applications.Items[0].Token;
                }
                
                var issues = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
                {
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                    Paging = new PageRequestWithSort(1, 0),
                    AssignedTo = Core.AppContext.CurrentUser.Id,
                    ApplicationId = applicationId
                }).Issues;

                var recentIssues = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
                {
                    Paging = new PageRequestWithSort(1, 8, "FriendlyId", true),
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                    ApplicationId = applicationId
                }).Issues;

                viewModel.TestIssueId = recentIssues.Items.FirstOrDefault(i => i.TestIssue).IfPoss(i => i.Id);

                var recentErrors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
                {
                    Paging = new PageRequestWithSort(1, 8, "FriendlyId", true),
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                    ApplicationId = applicationId
                }).Errors;

                var selectedApplication = applicationId.IsNotNullOrEmpty()
                    ? applications.Items.FirstOrDefault(a => a.FriendlyId == applicationId.GetFriendlyId())
                    : null;

                viewModel.LastErrorDisplayed = recentErrors.PagingStatus.TotalItems > 0 ? int.Parse(recentErrors.Items.First().FriendlyId) : -1;
                viewModel.LastIssueDisplayed = recentIssues.PagingStatus.TotalItems > 0 ? int.Parse(recentIssues.Items.First().FriendlyId) : -1;
                viewModel.Stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest { OrganisationId = Core.AppContext.CurrentUser.OrganisationId }).Statistics ?? new Statistics();
                viewModel.Stats.CurrentUserIssueCount = issues.PagingStatus.TotalItems;
				viewModel.RecentIssues = IssueItemViewModel.ConvertSimple(recentIssues.Items, Core.GetUsers().Items);
                viewModel.SelectedApplicationId = selectedApplication == null ? null : selectedApplication.FriendlyId;
                viewModel.SelectedApplicationName = selectedApplication == null ? null : selectedApplication.Name;
                viewModel.Applications = applications.Items;
                viewModel.RecentErrors = recentErrors.Items.Select(e => new ErrorInstanceViewModel
	            {
		            Error = e,
					ApplicationName = GetApplicationName(applications.Items, e.ApplicationId)
	            }).ToList();
            }
            else
            {
                viewModel.Stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest { OrganisationId = Core.AppContext.CurrentUser.OrganisationId }).Statistics ?? new Statistics();
            }

            return View(viewModel);
        }

        public ActionResult Update(int lastErrorDisplayed, int lastIssueDisplayed, string applicationId)
		{
			var applications = Core.GetApplications();

            var issues = lastIssueDisplayed == -1 ? null : _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
			{
				Paging = new PageRequestWithSort(1, 50, "FriendlyId", true),
				OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                LastFriendlyId = lastIssueDisplayed,
                UserTimezoneId = AppContext.CurrentUser.EffectiveTimezoneId(),
                ApplicationId = applicationId
			}).Issues;

			var errors = lastErrorDisplayed == -1 ? null : _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
			{
                Paging = new PageRequestWithSort(1, 50, "FriendlyId", true),
				OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                LastFriendlyId = lastErrorDisplayed,
				UserTimezoneId = AppContext.CurrentUser.EffectiveTimezoneId(),
                ApplicationId = applicationId
			}).Errors;

			var result = new
			{
                issues = issues == null ? new string[] { } : IssueItemViewModel.ConvertSimple(issues.Items, Core.GetUsers().Items).Select(i => RenderPartial("Dashboard/Issue", i)),
                errors = errors == null ? new string[] { } : errors.Items.Select(e => new ErrorInstanceViewModel
	            {
		            Error = e,
					ApplicationName = GetApplicationName(applications.Items, e.ApplicationId)
	            }).Select(e => RenderPartial("Dashboard/Error", e)),
                lastErrorDisplayed = errors != null && errors.PagingStatus.TotalItems > 0 ? int.Parse(errors.Items.First().FriendlyId) : lastErrorDisplayed,
                lastIssueDisplayed = issues != null && issues.PagingStatus.TotalItems > 0 ? int.Parse(issues.Items.First().FriendlyId) : lastIssueDisplayed
			};

			return new JsonSuccessResult(result, allowGet: true);
		}

		private string GetApplicationName(IEnumerable<Application> applications, string applicationId)
		{
			var application = applications.FirstOrDefault(a => a.Id == applicationId);
			return application == null ? "Not Found" : application.Name;
		}
    }
}
