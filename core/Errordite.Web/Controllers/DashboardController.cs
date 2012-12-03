using System.Linq;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
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

        public DashboardController(IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery, 
            IGetApplicationIssuesQuery getApplicationIssuesQuery, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery)
        {
            _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
            _getApplicationIssuesQuery = getApplicationIssuesQuery;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
        }

        [ImportViewData]
        public ActionResult Index(string query)
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
                    Paging = new PageRequestWithSort(1, 0)
                }).Issues;

                var recentIssues = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
                {
                    Paging = new PageRequestWithSort(1, 5, "FriendlyId", true),
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                    Name = query
                }).Issues;

                viewModel.TestIssueId = recentIssues.Items.FirstOrDefault(i => i.TestIssue).IfPoss(i => i.Id);

                var recentErrors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
                {
                    Paging = new PageRequestWithSort(1, 10, sortDescending: true),
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                    Query = query
                }).Errors;

                viewModel.Stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest { OrganisationId = Core.AppContext.CurrentUser.OrganisationId }).Statistics ?? new Statistics();
                viewModel.Stats.CurrentUserIssueCount = issues.PagingStatus.TotalItems;
                viewModel.RecentIssues = IssueItemViewModel.FromIssues(recentIssues.Items, applications.Items, Core.GetUsers().Items);
                viewModel.RecentErrors = recentErrors.Items.Select(e => new ErrorInstanceViewModel { Error = e }).ToList();
            }
            else
            {
                viewModel.Stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest { OrganisationId = Core.AppContext.CurrentUser.OrganisationId }).Statistics ?? new Statistics();
            }

            return View(viewModel);
        }
    }
}
