using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Errordite.Core.Paging
{
    [Serializable]
    public class PagingViewModel
    {
        public PagingViewModel()
        {
            PagingId = PagingConstants.DefaultPagingId;
        }

        public int FirstItem { get; set; }
        public int LastItem { get; set; }
        public int TotalItems { get; set; }
        public bool NoItems { get; set; }
        public int CurrentPage { get; set; }
        public int LastPage { get; set; }
        public IEnumerable<PageSelector> PageSelectors { get; set; }
        public string PagingId { get; set; }
        public int PageSize { get; set; }
        public int? AltPageSize { get; set; }
        public string ItemNamePlural { get; set; }
        public IEnumerable<SelectListItem> PageSizes { get; set; }
        public string Tab { get; set; }
    }

    [Serializable]
    public class PageSelector
    {
        public bool Current { get; set; }
        public int PageId { get; set; }
        public bool PrependEllipsis { get; set; }
    }
}