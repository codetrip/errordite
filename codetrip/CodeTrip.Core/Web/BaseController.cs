using System.Collections.Generic;
using System.Web.Mvc;
using CodeTrip.Core.Auditing;
using CodeTrip.Core.Paging;

namespace CodeTrip.Core.Web
{
    public abstract class BaseController : AuditingController
    {
        #region Paging stuff

        /// <summary>
        /// Call this to get a Paging object to pass to the service tier when (as is usual) there is only
        /// one paged table on the page.  Requires the PagingViewAttribute to be passed on the Controller Action.
        /// </summary>
        public PageRequestWithSort GetSinglePagingRequest()
        {
            return PagingInfos[PagingConstants.DefaultPagingId];
        }

        /// <summary>
        /// Call this to get a Paging object to pass to the service tier when there are 2 or more 
        /// paged tables on the page.  The pagingId should identify which table we are talking about.
        /// Requires the PagingViewAttribute to be passed on the Controller Action.
        /// </summary>
        public PageRequestWithSort GetPagingRequestById(string pagingId)
        {
            return PagingInfos[pagingId];
        }

        /// <summary>
        /// The collection of PagingInfos, initialised by the PagingViewAttribute.
        /// </summary>
        public Dictionary<string, PageRequestWithSort> PagingInfos
        {
            get;
            set;
        }

        public bool RegisterPagingStatus(string pagingId, PagingStatus pagingStatus, PageRequestWithSort pagingRequest, out ActionResult overrideActionResult)
        {
            return RegisterPagingStatus(pagingId, pagingStatus, pagingRequest, null, out overrideActionResult);
        }

        /// <summary>
        /// Registers Pagination returned by the service tier.  Use when there are multiple paged tables and you need to
        /// specify an id.
        /// </summary>
        public bool RegisterPagingStatus(string pagingId, PagingStatus pagination, PageRequestWithSort pagingRequest, string itemNamePlural, out ActionResult result)
        {
            string queryStringParameter = PagingConstants.QueryStringParameters.PageNumber;
            if (pagingId != PagingConstants.DefaultPagingId)
            {
                queryStringParameter += pagingId;
            }

            int correctPageNumber;
            if (IsPaginationInvalid(pagingRequest.PageNumber, pagination.TotalPages, out correctPageNumber))
            {
                var parameters = new KeyValuePair<string, string>(queryStringParameter, correctPageNumber.ToString());
                result = new RedirectResult( HttpContext.Request.Url.ChangeQueryString(parameters));
                return false;
            }
            var viewModel = PagingViewModelGenerator.Generate(pagingId, pagination, pagingRequest);
            viewModel.ItemNamePlural = itemNamePlural;
            ViewData["pagination" + pagingId] = viewModel;

            result = null;
            return true;
        }

        /// <summary>
        /// Registers Pagination returned by the service tier.  Use when there is only one paged table.
        /// </summary>
        public bool RegisterPagingStatus(PagingStatus pagination, PageRequestWithSort pagingRequest, out ActionResult result)
        {
            return RegisterPagingStatus(PagingConstants.DefaultPagingId, pagination, pagingRequest, out result);
        }

        public bool RegisterPagingStatus(PagingStatus pagination, PageRequestWithSort pagingRequest, string itemNamePlural, out ActionResult result)
        {
            return RegisterPagingStatus(PagingConstants.DefaultPagingId, pagination, pagingRequest, itemNamePlural, out result);
        }

        private bool IsPaginationInvalid(int currentPage, int totalPages, out int correctPageNumber)
        {
            if (currentPage <= 0)
            {
                correctPageNumber = 1;
                return true;
            }

            if (currentPage > totalPages && currentPage > 1)
            {
                if (totalPages == 0)
                    correctPageNumber = currentPage - 1;
                else
                    correctPageNumber = totalPages;
                return true;
            }

            correctPageNumber = currentPage;
            return false;
        }

        /// <summary>
        /// Sets (usually by Ioc) the default PagingViewModelGenerator.
        /// </summary>
        public IPagingViewModelGenerator PagingViewModelGeneratorSetter
        {
            private get;
            set;
        }

        /// <summary>
        /// Gets the PagingViewModelGenerator to use.  Can be overrideen in inherited controllers to allow different functionality.
        /// </summary>
        protected virtual IPagingViewModelGenerator PagingViewModelGenerator
        {
            get { return PagingViewModelGeneratorSetter; }
        }

        public IPagingConfiguration PagingConfiguration
        {
            get;
            set;
        }

        #endregion
    }
}