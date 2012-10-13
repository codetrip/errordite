using System;
using System.Linq;
using CodeTrip.Core.Paging;

namespace CodeTrip.Core.Extensions
{
    public static class PageExtensions
    {
        public static Page<T> Filter<T>(this Page<T> original, Func<T, bool> filter, PageRequestWithSort paging)
        {
            var items = original.Items.Where(filter);
            return new Page<T>(items.Skip((paging.PageNumber - 1) * paging.PageSize).Take(paging.PageSize).ToList(), new PagingStatus(original.PagingStatus.PageSize, original.PagingStatus.PageNumber, items.Count()));
        }

        public static Page<T> AdjustSetForPaging<T>(this Page<T> original, PageRequestWithSort paging)
        {
            return new Page<T>(original.Items.Skip((paging.PageNumber - 1) * paging.PageSize).Take(paging.PageSize).ToList(), new PagingStatus(paging.PageSize, paging.PageNumber, original.PagingStatus.TotalItems));
        }
    }
}