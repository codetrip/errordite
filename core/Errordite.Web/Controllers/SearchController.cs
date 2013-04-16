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
using Errordite.Web.Extensions;
using CodeTrip.Core.Extensions;

namespace Errordite.Web.Controllers
{
    public class SearchController : ErrorditeController
    {
		private readonly IGetApplicationIssuesQuery _getApplicationIssuesQuery;
		private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IGetIssueQuery _getIssueQuery;

	    public SearchController(IGetApplicationIssuesQuery getApplicationIssuesQuery, IGetApplicationErrorsQuery getApplicationErrorsQuery, IGetIssueQuery getIssueQuery)
	    {
		    _getApplicationIssuesQuery = getApplicationIssuesQuery;
		    _getApplicationErrorsQuery = getApplicationErrorsQuery;
	        _getIssueQuery = getIssueQuery;
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
				viewModel.Issues = IssueItemViewModel.Convert(issues.Items, applications.Items, Core.GetUsers().Items);
				viewModel.Errors = errors.Items.Select(e => new ErrorInstanceViewModel
				{
					Error = e,
					ApplicationName = GetApplicationName(applications.Items, e.ApplicationId)
				}).ToList();

			    int issueId;
			    if (int.TryParse(q, out issueId))
			    {
			        var issue =
			            _getIssueQuery.Invoke(new GetIssueRequest() {CurrentUser = Core.AppContext.CurrentUser, IssueId = issueId.ToString()}).Issue;
			        if (issue != null && viewModel.IssueTotal == 0 && viewModel.ErrorTotal == 0)
			            return Redirect(Url.Issue(issueId.ToString()));

                    //as currently setup, the issues / errors are indexed with the SimpleAnalyzer, which just drops terms
                    //with all non-alpha (e.g. all numeric) characters.  This is probably not a good thing but this
                    //"click to go to issue" thing will never happen as currently set up
			        viewModel.IssueWithMatchingId = issue;
			    }
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
