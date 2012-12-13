using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Issues.Queries;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using Errordite.Web.Models.Search;

namespace Errordite.Web.Controllers
{
    public class SearchController : ErrorditeController
    {
		private readonly IGetApplicationIssuesQuery _getApplicationIssuesQuery;
		private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;

	    public SearchController(IGetApplicationIssuesQuery getApplicationIssuesQuery, IGetApplicationErrorsQuery getApplicationErrorsQuery)
	    {
		    _getApplicationIssuesQuery = getApplicationIssuesQuery;
		    _getApplicationErrorsQuery = getApplicationErrorsQuery;
	    }

	    public ActionResult Index(string q)
        {
			var viewModel = new SearchViewModel
			{
				Query = q
			};

			var applications = Core.GetApplications();

			if (applications.PagingStatus.TotalItems > 0)
			{
				var issues = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
				{
					Paging = new PageRequestWithSort(1, 10),
					OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
					Name = q
				}).Issues;

				var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
				{
					Paging = new PageRequestWithSort(1, 10),
					OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
					Query = q
				}).Errors;

				viewModel.IssueTotal = issues.PagingStatus.TotalItems;
				viewModel.ErrorTotal = errors.PagingStatus.TotalItems;
				viewModel.Issues = IssueItemViewModel.FromIssues(issues.Items, applications.Items, Core.GetUsers().Items);
				viewModel.Errors = errors.Items.Select(e => new ErrorInstanceViewModel
				{
					Error = e,
					ApplicationName = GetApplicationName(applications.Items, e.ApplicationId)
				}).ToList();
			}

			return View(viewModel);
        }

		private string GetApplicationName(IEnumerable<Application> applications, string applicationId)
		{
			var application = applications.FirstOrDefault(a => a.Id == applicationId);
			return application == null ? "Not Found" : application.Name;
		}
    }
}
