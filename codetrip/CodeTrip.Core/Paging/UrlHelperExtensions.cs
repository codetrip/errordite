using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Paging
{
    public static class UriExtensions
    {
        public static string ChangeQueryString(this Uri uri, params KeyValuePair<string, string>[] replacementValues)
        {
            var uriBuilder = new UriBuilder(uri);

            var queryString = HttpUtility.ParseQueryString(uriBuilder.Query);

            //validate and encode incoming url
            foreach (string key in queryString.AllKeys)
            {
                //make sure the users query is url encoded
                queryString[key] = HttpUtility.UrlEncode(queryString[key]);
            }

            foreach (var replacementValue in replacementValues)
            {
                if (replacementValue.Value.IsNotNullOrEmpty())
                    queryString[replacementValue.Key] = replacementValue.Value;
            }

            uriBuilder.Query = queryString.Cast<string>().Where(k => !k.IsNullOrEmpty()).StringConcat(k => "{0}={1}&".FormatWith(k, queryString[k]));

            var url = "{0}{1}".FormatWith(uriBuilder.Path, uriBuilder.Query.TrimEnd('&').Replace("true,false", "true"));

            return url;
        }
    }

    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString SortLink(this HtmlHelper helper, string pagingId, string linkText, string sort, bool sortDescending, string tab = null)
        {
            if (pagingId == PagingConstants.DefaultPagingId)
                pagingId = string.Empty;

            return GetSlightlyChangedLink(helper, linkText,
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageSort + pagingId, sort),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageSortDescending + pagingId, sortDescending.ToString()),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageTab, tab));
        }

        public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pagingId, int pageNumber, string tab = null)
        {
            if (pagingId == PagingConstants.DefaultPagingId)
                pagingId = string.Empty;

            return GetSlightlyChangedLink(helper, linkText,
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageNumber + pagingId, pageNumber.ToString()),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageTab, tab));
        }

        public static string ReplacementPageLink(this HtmlHelper helper, string pagingId, string tab = null)
        {
            if (pagingId == PagingConstants.DefaultPagingId)
                pagingId = string.Empty;

            return helper.ViewContext.HttpContext.Request.Url.ChangeQueryString(
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageSize + pagingId, "[PGSZ]"),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageNumber + pagingId, "[PGNO]"),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageTab, tab));
        }

        private static MvcHtmlString GetSlightlyChangedLink(HtmlHelper helper, string linkText, params KeyValuePair<string, string>[] replacementValues)
        {
            var changedUrl = helper.ViewContext.HttpContext.Request.Url.ChangeQueryString(replacementValues);
            return MvcHtmlString.Create("<a href='{0}'>{1}</a>".FormatWith(changedUrl, linkText));
        }
    }
}