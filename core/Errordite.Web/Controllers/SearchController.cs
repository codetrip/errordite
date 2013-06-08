using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Errordite.Core.Domain.Error;
using Errordite.Core.Paging;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Session;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using Errordite.Web.Models.Search;
using Errordite.Web.Extensions;
using Errordite.Core.Extensions;

namespace Errordite.Web.Controllers
{
	[Authorize]
    public class SearchController : ErrorditeController
    {
		private readonly IGetApplicationIssuesQuery _getApplicationIssuesQuery;
		private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IGetIssueQuery _getIssueQuery;
	    private readonly IAppSession _session;

	    public SearchController(IGetApplicationIssuesQuery getApplicationIssuesQuery,
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IGetIssueQuery getIssueQuery, 
            IAppSession session)
	    {
		    _getApplicationIssuesQuery = getApplicationIssuesQuery;
		    _getApplicationErrorsQuery = getApplicationErrorsQuery;
	        _getIssueQuery = getIssueQuery;
	        _session = session;
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
			    int searchedIssueId;
                if (int.TryParse(q, out searchedIssueId))
                {
                    var issue = _session.Raven.Load<Issue>(searchedIssueId);
                    if (issue != null)
                    {
                        return Redirect(Url.Issue(searchedIssueId.ToString()));
                    }
                }

				var issues = _getApplicationIssuesQuery.Invoke(new GetApplicationIssuesRequest
				{
					Paging = new PageRequestWithSort(1, 10),
					OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
					Query = q,
                    ApplicationId = CurrentApplication.IfPoss(a => a.Id),
				}).Issues;

				var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
				{
					Paging = new PageRequestWithSort(1, 10),
					OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
					Query = q,
                    ApplicationId = CurrentApplication.IfPoss(a => a.Id),
				}).Errors;

				viewModel.IssueTotal = issues.PagingStatus.TotalItems;
				viewModel.ErrorTotal = errors.PagingStatus.TotalItems;
				viewModel.Issues = IssueItemViewModel.ConvertSimple(issues.Items, Core.GetUsers().Items, Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId);
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
