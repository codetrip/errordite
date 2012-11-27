using System;
using System.Linq;
using System.Web.Mvc;

namespace CodeTrip.Core.Paging
{
    public class PagingViewModelGenerator : IPagingViewModelGenerator
    {
        protected readonly IPagingConfiguration PagingConfiguration;

        public PagingViewModelGenerator(IPagingConfiguration pagingConfiguration)
        {
            PagingConfiguration = pagingConfiguration;
        }

        public PagingViewModel Generate(string pagingId, PagingStatus pagingStatus, PageRequestWithSort request)
        {
            if (pagingStatus == null)
                return new PagingViewModel { NoItems = true };

            int itemCount = pagingStatus.TotalItems;

            if (itemCount == 0)
                return new PagingViewModel { NoItems = true };

            int pageCount = itemCount / pagingStatus.PageSize;

            if (itemCount % pagingStatus.PageSize > 0)
                pageCount++;

            int selectedPage = Math.Max(1, Math.Min(pagingStatus.PageNumber, pageCount));

            int firstPageSelector;
            if (selectedPage <= PagingConfiguration.PageSelectorCount / 2) //if the selected page is before the middle of the visible selectors
                firstPageSelector = 1;
            else if (pageCount - selectedPage <= PagingConfiguration.PageSelectorCount / 2) //if the selected page is after the middle of the visible selectors
                firstPageSelector = Math.Max(1, pageCount - PagingConfiguration.PageSelectorCount + 1);
            else
                firstPageSelector = selectedPage - (PagingConfiguration.PageSelectorCount / 2);  //else - the selected page is in the middle

            int lastPageSelector = Math.Min(firstPageSelector + PagingConfiguration.PageSelectorCount - 1, pageCount);

            //int firstItemIndex = pagingStatus.PageSize * (selectedPage - 1);
            //int itemsOnSelectedPage = Math.Min(pagingStatus.PageSize, itemCount - firstItemIndex);

            var ret = new PagingViewModel
            {
                PageSize = pagingStatus.PageSize,
                AltPageSize = null,
                CurrentPage = selectedPage,
                LastPage = pageCount,
                PageSelectors = Enumerable.Range(firstPageSelector, lastPageSelector - firstPageSelector + 1).Select(
                 i => new PageSelector
                    {
                        Current = i == selectedPage, 
                        PageId = i, 
                        PrependEllipsis = i == firstPageSelector && i > 2
                    }),
                PagingId = pagingId,
                FirstItem = pagingStatus.FirstItem,
                LastItem = pagingStatus.LastItem,
                TotalItems = pagingStatus.TotalItems,
                PageSizes = PagingConfiguration.PageSizes.Select(size => new SelectListItem { Selected = size == pagingStatus.PageSize, Text = size.ToString(), Value = size.ToString()})
            };

            if (firstPageSelector > 1)
            {
                ret.PageSelectors = new[] 
                { 
                    new PageSelector
                    {
                        Current = false, 
                        PageId = 1, 
                        PrependEllipsis = false
                    } 
                }.Union(ret.PageSelectors);
            }

            if (ret.LastPage > lastPageSelector)
            {
                ret.PageSelectors =
                    ret.PageSelectors.Union(new[]
                        {
                            new PageSelector
                            {
                                Current = pageCount == selectedPage,
                                PageId = pageCount,
                                PrependEllipsis = pageCount > lastPageSelector + 1
                            }
                        });
            }

            ExtraViewModelBuilding(ret, request);

            return ret;
        }

        protected virtual void ExtraViewModelBuilding(PagingViewModel paginationViewModel, PageRequestWithSort request)
        {}
    }

    public interface IPagingViewModelGenerator
    {
        PagingViewModel Generate(string pagingId, PagingStatus pagingStatus, PageRequestWithSort request);
    }
}