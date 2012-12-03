using System;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core;
using Errordite.Core.Errors.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Errors;
using System.Linq;
using CodeTrip.Core.Extensions;
using Errordite.Web.Extensions;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class ErrorsController : ErrorditeController
    {
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;

        public ErrorsController(IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IPagingViewModelGenerator pagingViewModelGenerator)
        {
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _pagingViewModelGenerator = pagingViewModelGenerator;
        }

        [
        PagingView(DefaultSort = CoreConstants.SortFields.TimestampUtc, DefaultSortDescending = true), 
        ExportViewData,
        ImportViewData,
        StoreQueryInCookie(WebConstants.CookieSettings.ErrorSearchCookieKey)
        ]
        public ActionResult Index(ErrorCriteriaPostModel postModel)
        {
            var viewModel = new ErrorPageViewModel
            {
                ErrorsViewModel = new ErrorCriteriaViewModel
                {
                    Action = "index",
                    Controller = "errors"
                }
            };

            var applications = Core.GetApplications();
            var pagingRequest = GetSinglePagingRequest();

            if (applications.PagingStatus.TotalItems > 0)
            {
                var request = new GetApplicationErrorsRequest
                {
                    ApplicationId = postModel.ApplicationId,
                    Paging = pagingRequest,
                    Query = postModel.Query,
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                    UserTimezoneId = AppContext.CurrentUser.EffectiveTimezoneId(),
                };

                if (postModel.DateRange.IsNotNullOrEmpty())
                {
                    string[] dates = postModel.DateRange.Split('|');

                    DateTime startDate;
                    DateTime endDate;

                    if (DateTime.TryParse(dates[0], out startDate) && DateTime.TryParse(dates[1], out endDate))
                    {
                        request.StartDate = startDate;
                        request.EndDate = endDate.AddDays(1).AddMinutes(-1);
                    }
                }

                var errors = _getApplicationErrorsQuery.Invoke(request);

                viewModel.ErrorsViewModel.DateRange = postModel.DateRange;
                viewModel.ErrorsViewModel.Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, errors.Errors.PagingStatus, pagingRequest);
                viewModel.ErrorsViewModel.Errors = errors.Errors.Items.Select(e => new ErrorInstanceViewModel { Error = e }).ToList();
                viewModel.ErrorsViewModel.ApplicationId = postModel.ApplicationId;
                viewModel.ApplicationName = postModel.ApplicationId.IsNullOrEmpty() ? Resources.Application.AllApplications : applications.Items.First(a => a.FriendlyId == postModel.ApplicationId).Name;
                viewModel.ErrorsViewModel.Applications = applications.Items.ToSelectList(a => a.FriendlyId, a => a.Name, u => u.FriendlyId == postModel.ApplicationId, Resources.Shared.Application, string.Empty, SortSelectListBy.Text);
            }
            else
            {
                ErrorNotification(Resources.Application.No_Applications);
                return Redirect(Url.AddApplication());
            }

            return View(viewModel);
        }
    }
}
