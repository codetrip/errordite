using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Paging;
using Errordite.Core;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Web;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Errors;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Navigation;
using Domain = Errordite.Core.Domain.Error;

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
                viewModel.ErrorsViewModel.Errors = errors.Errors.Items.Select(e => new ErrorInstanceViewModel
	                {
						Error = e,
						//IsGetMethod = e.ContextData.ContainsKey("Request.HttpMethod") && e.ContextData["Request.HttpMethod"].ToLowerInvariant() == "get"
	                }).ToList();
            }
            else
            {
                ErrorNotification(Resources.Application.No_Applications);
                return Redirect(Url.AddApplication());
            }

            return View(viewModel);
        }

		public ActionResult Replay(string id)
		{
			var error = Core.Session.Raven.Load<Domain.Error>(Domain.Error.GetId(id));

			if (error != null)
			{
				var url = error.Url;

				try
				{
					if (Core.AppContext.CurrentUser.ActiveOrganisation.ReplayReplacements != null)
					{
						url = Core.AppContext.CurrentUser.ActiveOrganisation.ReplayReplacements.Aggregate(url, (current, replacement) => current.Replace(replacement.Find, replacement.Replace));
					}

					var response = SynchronousWebRequest.To(url)
						.WithMethod(GetContextDataItem(error.ContextData, "Request.HttpMethod"))
						.WithUserAgent(error.UserAgent)
						.FromReferer(GetContextDataItem(error.ContextData, "Request.Referrer"))
						.Accept(GetContextDataItem(error.ContextData, "Request.Header.Accept"))
						.Host(GetContextDataItem(error.ContextData, "Request.Header.Host"))
						.Connection(GetContextDataItem(error.ContextData, "Request.Header.Connection"))
						.AddHeader("Cookie", GetContextDataItem(error.ContextData, "Request.Header.Cookie"))
						.AddHeader("Accept-Language", GetContextDataItem(error.ContextData, "Request.Header.Accept-Language"))
						.AddHeader("Accept-Encoding", GetContextDataItem(error.ContextData, "Request.Header.Accept-Encoding"))
						.GetResponse();

					return Content(response.Body);
				}
				catch (Exception e)
				{
					return View(new ReplayErrorViewModel
					{
						Error = e.Message,
						Url = url,
						ErrorId = id
					});
				}
			}

			return View(new ReplayErrorViewModel
			{
				Error = "Error with Id {0} was not found".FormatWith(id),
				ErrorId = id
			});
		}

		private string GetContextDataItem(Dictionary<string, string> data, string key)
		{
			if (data.ContainsKey(key))
				return data[key];

			return null;
		}
    }
}
