using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Errordite.Core;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Paging;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Dashboard;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using Errordite.Core.Extensions;
using Errordite.Web.Models.Navigation;

namespace Errordite.Web.Controllers
{
	[Authorize]
    public class DashboardController : ErrorditeController
    {
        private readonly IGetApplicationIssuesQuery _getApplicationIssuesQuery;
        private readonly IGetOrganisationStatisticsQuery _getOrganisationStatisticsQuery;
        private readonly IGetDashboardReportQuery _getDashboardReportQuery;
        private readonly IGetIssueQuery _getIssueQuery;
		private readonly IGetActivityLogQuery _getActivityLogQuery;
		private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
		private readonly IMostRecurringIssuesForDateQuery _getMostRecurringIssuesForDateQuery;

        public DashboardController(IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery, 
            IGetApplicationIssuesQuery getApplicationIssuesQuery, 
            IGetDashboardReportQuery getDashboardReportQuery, 
            IGetIssueQuery getIssueQuery, 
			IGetActivityLogQuery getActivityLogQuery, 
			IGetApplicationErrorsQuery getApplicationErrorsQuery,
			IMostRecurringIssuesForDateQuery getMostRecurringIssuesForDateQuery)
        {
            _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
            _getApplicationIssuesQuery = getApplicationIssuesQuery;
            _getDashboardReportQuery = getDashboardReportQuery;
            _getIssueQuery = getIssueQuery;
            _getActivityLogQuery = getActivityLogQuery;
	        _getApplicationErrorsQuery = getApplicationErrorsQuery;
	        _getMostRecurringIssuesForDateQuery = getMostRecurringIssuesForDateQuery;
        }

        [ImportViewData, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.Dashboard)]
        public ActionResult Index()
        {
            var curentApplication = CurrentApplication;
            var applicationId = curentApplication == null ? null : curentApplication.Id;
            var viewModel = new DashboardViewModel();
            var applications = Core.GetApplications();
	        var preferences = CookieManager.Get(WebConstants.CookieSettings.DashboardCookieKey);
			var pref = preferences.IsNullOrEmpty() || !preferences.Contains("|") ? null : preferences.Split('|');

			viewModel.ShowMe = pref == null ? "1" : pref[0];
	        viewModel.PageSize = pref == null ? 10 : int.Parse(pref[1]); 
        
            if(applications.PagingStatus.TotalItems > 0)
            {
                viewModel.HasApplications = true;

                if (applications.Items.Count == 1)
                {
                    viewModel.SingleApplicationId = applications.Items[0].Id;
                    viewModel.SingleApplicationToken = applications.Items[0].Token;
                }

				var showMe = DashboardViewModel.Sorting.FirstOrDefault(s => s.Id == viewModel.ShowMe) ?? new DashboardSort("1", "", true, "LastErrorUtc");

				if (showMe.Id == "5")
				{
					var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
					{
						Paging = new PageRequestWithSort(1, viewModel.PageSize, "TimestampUtc", true),
						OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
						ApplicationId = applicationId
					}).Errors;

					viewModel.Errors = errors.Items.Select(e => new ErrorInstanceViewModel
					{
						Error = e,
						ApplicationName = GetApplicationName(applications.Items, e.ApplicationId),
					}).ToList();

					viewModel.ShowIntro = errors.PagingStatus.TotalItems <= 5 && applications.Items.Count == 1;
				}
				else
				{
					var issues = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
					{
						Paging = new PageRequestWithSort(1, viewModel.PageSize, showMe.SortField, showMe.SortDescending),
						OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
						ApplicationId = applicationId
					}).Issues;

					viewModel.TestIssueId = issues.Items.FirstOrDefault(i => i.TestIssue).IfPoss(i => i.Id);
					viewModel.Issues = IssueItemViewModel.ConvertSimple(issues.Items, Core.GetUsers().Items, Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId, showMe.Id != "2");
					viewModel.ShowIntro = issues.PagingStatus.TotalItems <= 3 && applications.Items.Count == 1;
				}

                var selectedApplication = applicationId.IsNotNullOrEmpty()
                    ? applications.Items.FirstOrDefault(a => a.FriendlyId == applicationId.GetFriendlyId())
                    : null;

                viewModel.SelectedApplicationId = selectedApplication == null ? null : selectedApplication.FriendlyId;
                viewModel.SelectedApplicationName = selectedApplication == null ? null : selectedApplication.Name;
                viewModel.Applications = applications.Items;
                viewModel.UrlGetter = GetDashboardUrl;
	            viewModel.ShowMeOptions = DashboardViewModel.Sorting.ToSelectList(s => s.Id, s => s.DisplayName, s => s.Id == viewModel.ShowMe);
	            viewModel.PageSizes = new List<SelectListItem>
		        {
			        new SelectListItem {Text = "10", Value = "10", Selected = viewModel.PageSize == 10},
			        new SelectListItem {Text = "20", Value = "20", Selected = viewModel.PageSize == 20},
			        new SelectListItem {Text = "30", Value = "30", Selected = viewModel.PageSize == 30}
		        };
            }
            else
            {
                ConfirmationNotification("You do not currently have any applications, please create an application to begin using Errordite.");
                return Redirect(Url.AddApplication(false));
            }

            return View(viewModel);
        }

		private string GetApplicationName(IEnumerable<Application> applications, string applicationId)
		{
			var application = applications.FirstOrDefault(a => a.Id == applicationId);
			return application == null ? "Not Found" : application.Name;
		}

		public ActionResult IssueBreakdown(string dateFormat)
		{
			var date = DateTime.ParseExact(dateFormat.Substring(0, 24), "ddd MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture);
			var currentApplication = CurrentApplication;
			var applicationId = currentApplication == null ? null : currentApplication.Id;

			var data = _getMostRecurringIssuesForDateQuery.Invoke(new GetMostRecurringIssuesForDateRequest
			{
				ApplicationId = applicationId,
				Date = date,
			}).Data;

			return new JsonSuccessResult(data, allowGet: true);
		}

        public ActionResult Update(string mode, string showMe, int pageSize)
        {
	        object feed = null;
			object errors = null;
			object stats = null;
	        var liveErrorFeed = false;

			var currentApplication = CurrentApplication;
			var applicationId = currentApplication == null ? null : currentApplication.Id;

			if (mode.IsIn("feed", "undefined"))
			{
				CookieManager.Set(WebConstants.CookieSettings.DashboardCookieKey, "{0}|{1}".FormatWith(showMe, pageSize), null);

				var sort = DashboardViewModel.Sorting.First(s => s.Id == showMe);

				if (sort.Id == "5")
				{
					liveErrorFeed = true;
					var applications = Core.GetApplications();
					var items = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
					{
						Paging = new PageRequestWithSort(1, pageSize, "TimestampUtc", true),
						OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
						ApplicationId = applicationId
					}).Errors;

					feed = items == null ? new string[] { } : items.Items.Select(e => new ErrorInstanceViewModel
					{
						Error = e,
						ApplicationName = GetApplicationName(applications.Items, e.ApplicationId),
					}).Select(e => RenderPartial("Dashboard/Error", e));
				}
				else
				{
					var items = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
					{
						Paging = new PageRequestWithSort(1, pageSize, sort.SortField, sort.SortDescending),
						OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
						ApplicationId = applicationId
					}).Issues.Items;

					feed = items == null || items.Count == 0 ?
						new string[] { } :
						IssueItemViewModel.ConvertSimple(items, Core.GetUsers().Items, Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId, sort.Id != "2")
										  .Select(i => RenderPartial("Dashboard/Issue", i));
				}
			}

			if (mode.IsIn("graphs", "undefined"))
			{
				stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest
				{
					ApplicationId = CurrentApplication == null ? null : CurrentApplication.Id
				}).Statistics ?? new Statistics();

				errors = _getDashboardReportQuery.Invoke(new GetDashboardReportRequest
				{
					ApplicationId = currentApplication == null ? null : currentApplication.Id
				}).Data;
			}

			return new JsonSuccessResult(new
			{
				feed,
				stats,
				errors,
				liveErrorFeed
			}, allowGet: true);
		}

		[GenerateBreadcrumbs(BreadcrumbId.ActivityLog)]
		public ActionResult Activity()
		{
			return View(GetActivityViewModel(1));
		}

		public ActionResult GetNextActivityPage(int pageNumber)
		{
			var model = GetActivityViewModel(pageNumber);
			return Content(string.Join("", model.Items.Select(i => RenderPartial("Issue/HistoryItem", i))));
		}

		public ActivityViewModelBase GetActivityViewModel(int pageNumber)
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
                    IssueLink = issueMemoizer.Get(h.IssueId).IfPoss(i => "<a href=\"{0}\">{1}</a>".FormatWith(Url.Issue(i.Id), i.Name), "DELETED"),
                    IssueId = IdHelper.GetFriendlyId(h.IssueId),
                };
            });

            var model = new ActivityViewModelBase
            {
                Paging = activity.PagingStatus,
                Items = items,
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
    }
}
