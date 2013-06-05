﻿using System;
using System.Web.Mvc;
using Errordite.Core.Paging;
using Errordite.Core;
using Errordite.Core.Errors.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Errors;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Navigation;

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
        //StoreQueryInCookie(WebConstants.CookieSettings.ErrorSearchCookieKey),
		GenerateBreadcrumbs(BreadcrumbId.Errors)
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
                    ApplicationId = CurrentApplication.IfPoss(a => a.Id),
                    Paging = pagingRequest,
                    Query = postModel.Query,
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                };

                if (postModel.DateRange.IsNotNullOrEmpty())
                {
                    string[] dates = postModel.DateRange.Split('|');

                    DateTime startDate;
                    DateTime endDate;

                    if (DateTime.TryParse(dates[0], out startDate) && DateTime.TryParse(dates[1], out endDate))
                    {
                        request.StartDate = startDate;
                        request.EndDate = endDate;
                        viewModel.ErrorsViewModel.DateRange = "{0} - {1}".FormatWith(startDate.ToString("MMMM d, yyyy"), endDate.ToString("MMMM d, yyyy"));
                    }
                }

                var errors = _getApplicationErrorsQuery.Invoke(request);
                
                viewModel.ErrorsViewModel.Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, errors.Errors.PagingStatus, pagingRequest);
                viewModel.ErrorsViewModel.Errors = errors.Errors.Items.Select(e => new ErrorInstanceViewModel { Error = e }).ToList();
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
