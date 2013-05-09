﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Errordite.Core;
using Errordite.Core.Paging;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Dashboard;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using Errordite.Core.Extensions;

namespace Errordite.Web.Controllers
{
	[Authorize, ValidateSubscriptionActionFilter]
    public class DashboardController : ErrorditeController
    {
        private readonly IGetApplicationIssuesQuery _getApplicationIssuesQuery;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IGetOrganisationStatisticsQuery _getOrganisationStatisticsQuery;
        private readonly IGetDashboardReportQuery _getDashboardReportQuery;
        private readonly IGetIssueQuery _getIssueQuery;
        private readonly IGetActivityLogQuery _getActivityLogQuery;

        public DashboardController(IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery, 
            IGetApplicationIssuesQuery getApplicationIssuesQuery, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IGetDashboardReportQuery getDashboardReportQuery, 
            IGetIssueQuery getIssueQuery, IGetActivityLogQuery getActivityLogQuery)
        {
            _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
            _getApplicationIssuesQuery = getApplicationIssuesQuery;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _getDashboardReportQuery = getDashboardReportQuery;
            _getIssueQuery = getIssueQuery;
            _getActivityLogQuery = getActivityLogQuery;
        }

        [ImportViewData]
        public ActionResult GetGraphData()
        {
            var curentApplication = CurrentApplication;
            var data = _getDashboardReportQuery.Invoke(new GetDashboardReportRequest
            {
                ApplicationId = curentApplication == null ? null : curentApplication.Id,
                OrganisationId = Core.AppContext.CurrentUser.OrganisationId
            }).Data;

            return new PlainJsonNetResult(data, true);
        }

        [ImportViewData, ExportViewData]
        public ActionResult Index()
        {
            var curentApplication = CurrentApplication;
            var applicationId = curentApplication == null ? null : curentApplication.Id;
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
                viewModel.Stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest { ApplicationId = applicationId }).Statistics ?? new Statistics();
                viewModel.Stats.CurrentUserIssueCount = issues.PagingStatus.TotalItems;
				viewModel.RecentIssues = IssueItemViewModel.ConvertSimple(recentIssues.Items, Core.GetUsers().Items);
                viewModel.SelectedApplicationId = selectedApplication == null ? null : selectedApplication.FriendlyId;
                viewModel.SelectedApplicationName = selectedApplication == null ? null : selectedApplication.Name;
                viewModel.Applications = applications.Items;
                viewModel.RecentErrors = recentErrors.Items.Select(e => new ErrorInstanceViewModel
	            {
		            Error = e,
					ApplicationName = GetApplicationName(applications.Items, e.ApplicationId),
	            }).ToList();
                viewModel.UrlGetter = GetDashboardUrl;
            }
            else
            {
                ConfirmationNotification("You do not currently have any applications, please create an application to begin using Errordite.");
                return Redirect(Url.AddApplication(false));
            }

            return View(viewModel);
        }

        public ActionResult Update(int lastErrorDisplayed, int lastIssueDisplayed)
		{
            var curentApplication = CurrentApplication;
			var applications = Core.GetApplications();
            var applicationId = curentApplication == null ? null : curentApplication.Id;

            var issues = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
			{
				Paging = new PageRequestWithSort(1, 50, "FriendlyId", true),
				OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                LastFriendlyId = lastIssueDisplayed,
                ApplicationId = applicationId
			}).Issues;

			var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
			{
                Paging = new PageRequestWithSort(1, 50, "FriendlyId", true),
				OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                LastFriendlyId = lastErrorDisplayed,
                ApplicationId = applicationId
			}).Errors;

			var result = new
			{
                issues = issues == null ? new string[] { } : IssueItemViewModel.ConvertSimple(issues.Items, Core.GetUsers().Items).Select(i => RenderPartial("Dashboard/Issue", i)),
                errors = errors == null ? new string[] { } : errors.Items.Select(e => new ErrorInstanceViewModel
	            {
		            Error = e,
					ApplicationName = GetApplicationName(applications.Items, e.ApplicationId),
	            }).Select(e => RenderPartial("Dashboard/Error", e)),
                lastErrorDisplayed = errors != null && errors.PagingStatus.TotalItems > 0 ? int.Parse(errors.Items.First().FriendlyId) : lastErrorDisplayed,
                lastIssueDisplayed = issues != null && issues.PagingStatus.TotalItems > 0 ? int.Parse(issues.Items.First().FriendlyId) : lastIssueDisplayed
			};

			return new JsonSuccessResult(result, allowGet: true);
		}

		public ActionResult Activity()
		{
			return View(GetActivityViewModel(1));
		}

		public ActionResult GetNextActivityPage(int pageNumber)
		{
			var model = GetActivityViewModel(pageNumber);
			return Content(string.Join("", model.Items.Select(i => RenderPartial("Issue/HistoryItem", i))));
		}

		public ActivityViewModel GetActivityViewModel(int pageNumber)
        {
            var curentApplication = CurrentApplication;
            var applicationId = curentApplication == null ? null : curentApplication.Id;
            var issueMemoizer = new LocalMemoizer<string, Issue>(id => _getIssueQuery.Invoke(new GetIssueRequest { CurrentUser = Core.AppContext.CurrentUser, IssueId = id }).Issue);
            var users = Core.GetUsers();
            var applications = Core.GetApplications();
            var activity = _getActivityLogQuery.Invoke(new GetActivityLogRequest
                {
                    Paging = new PageRequestWithSort(pageNumber, 20),
                    ApplicationId = applicationId
                }).Log;

            var selectedApplication = applicationId.IsNotNullOrEmpty()
                    ? applications.Items.FirstOrDefault(a => a.FriendlyId == applicationId.GetFriendlyId())
                    : null;

            var items = activity.Items.Select(h =>
            {
                var user = users.Items.FirstOrDefault(u => u.Id == h.UserId);
                return new IssueHistoryItemViewModel
                {
                    Message = h.GetMessage(users.Items, issueMemoizer, GetIssueLink),
                    VerbalTime = h.DateAddedUtc.ToVerbalTimeSinceUtc(Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId, true),
                    UserEmail = user != null ? user.Email : string.Empty,
                    Username = user != null ? user.FullName : string.Empty,
                    SystemMessage = h.SystemMessage,
                    IssueLink = issueMemoizer.Get(h.IssueId).IfPoss(i => "<a href=\"{0}\">{1}</a>".FormatWith(Url.Issue(i.Id), i.Name), "DELETED")
                };
            });

            var model = new ActivityViewModel
            {
                Paging = activity.PagingStatus,
                Items = items,
                Stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest { ApplicationId = applicationId }).Statistics ?? new Statistics(),
                Applications = applications.Items,
                SelectedApplicationId = selectedApplication == null ? null : selectedApplication.FriendlyId,
                SelectedApplicationName = selectedApplication == null ? null : selectedApplication.Name,
                UrlGetter = GetFeedUrl
            };

            return model;
        }

        private string GetFeedUrl(UrlHelper url, string applicationId)
        {
            return url.ActivityLog(applicationId);
        }

        private string GetDashboardUrl(UrlHelper url, string applicationId)
        {
            return url.Dashboard(applicationId);
        }

        private string GetIssueLink(string issueId)
        {
            return "<a href='{0}'>#{1}</a>".FormatWith(Url.Issue(issueId ?? "0"), IdHelper.GetFriendlyId(issueId ?? "0"));
        }

		private string GetApplicationName(IEnumerable<Application> applications, string applicationId)
		{
			var application = applications.FirstOrDefault(a => a.Id == applicationId);
			return application == null ? "Not Found" : application.Name;
		}
    }
}
