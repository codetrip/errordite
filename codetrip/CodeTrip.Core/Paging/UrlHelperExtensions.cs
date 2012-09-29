using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static MvcHtmlString SortLinks(this HtmlHelper helper, string pagingId, string sort)
        {
            return new MvcHtmlString(SortLink(helper, pagingId, sort, false).ToString() + SortLink(helper, pagingId, sort, true));
        }

        public static MvcHtmlString SortLink(this HtmlHelper helper, string pagingId, string sort, bool sortDescending, string tab = null, string customLinkText = null)
        {
            if (pagingId == PagingConstants.DefaultPagingId)
                pagingId = string.Empty;

            var sortIdParam = PagingConstants.QueryStringParameters.PageSort + pagingId;
            var sortDescendingParam = PagingConstants.QueryStringParameters.PageSortDescending + pagingId;

            var currentSort = helper.ViewContext.RequestContext.HttpContext.Request.QueryString[sortIdParam];
            var currentSortDescending =
                helper.ViewContext.RequestContext.HttpContext.Request.QueryString[sortDescendingParam];

            bool selected = (sort.Equals(currentSort, StringComparison.OrdinalIgnoreCase) &&
                             !(sortDescending ^ currentSortDescending.Equals("true", StringComparison.OrdinalIgnoreCase)));

            return GetSlightlyChangedLink(helper, customLinkText ?? (sortDescending ? "&#8659" : "&#8657"), 
                new[]{
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageSort + pagingId, sort),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageSortDescending + pagingId, sortDescending.ToString()),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageTab, tab)},
                sortDescending ? "Sort Descending" : "Sort Ascending",
                selected ? new[]{"sort-selected"} : new string[0],
                new {pgst = sort, pgsd = sortDescending});
        }

        public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pagingId, int pageNumber, string tab = null)
        {
            if (pagingId == PagingConstants.DefaultPagingId)
                pagingId = string.Empty;

            return GetSlightlyChangedLink(helper, linkText, 
                new[]{
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageNumber + pagingId, pageNumber.ToString()),
                new KeyValuePair<string, string>(PagingConstants.QueryStringParameters.PageTab, tab)});
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

        private static MvcHtmlString GetSlightlyChangedLink(HtmlHelper helper, string linkText, KeyValuePair<string, string>[] replacementValues, string tooltip = null, string[] cssClasses = null, object dataSet = null)
        {
            var changedUrl = helper.ViewContext.HttpContext.Request.Url.ChangeQueryString(replacementValues);
            var tb = new TagBuilder("a");
            tb.MergeAttribute("href", changedUrl);
            if (tooltip != null)
                tb.MergeAttribute("title", tooltip);
            foreach (var cssClass in cssClasses ?? new string[0])
                tb.AddCssClass(cssClass);
            tb.InnerHtml = linkText;
            tb.MergeAttributes(new DataAttributes(dataSet));
            return MvcHtmlString.Create(tb.ToString());

            //return MvcHtmlString.Create("<a href='{0}' {1} class='{2}'>{3}</a>".FormatWith(changedUrl, tooltip != null ? "title='{0}'".FormatWith(tooltip) : "", classString, linkText));
        }
    }

    /// <summary>
    /// Like the RouteValueDictionary in System.Routing, this dictionary
    /// can be created from an object by converting its propery values to 
    /// key value pairs.
    /// </summary>
    public class d : Dictionary<string, string>
    {
        public d(params object[] objects)
        {
            if (objects == null)
                return;

            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
                    if (!ContainsKey(GetPropName(prop)))
                        Add(GetPropName(prop), (prop.GetValue(obj, null) ?? "").ToString());
            }
        }

        public d()
        { }

        protected virtual string GetPropName(PropertyInfo prop)
        {
            var parameterNameAtt =
                (ParameterNameAttribute)
                prop.GetCustomAttributes(typeof(ParameterNameAttribute), false).FirstOrDefault();
            if (parameterNameAtt != null)
                return parameterNameAtt.ParameterName;
            return prop.Name;
        }
    }

    public class DataAttributes : d
    {
        public DataAttributes(params object[] objects) : base(objects)
        {
        }

        protected override string GetPropName(PropertyInfo prop)
        {
            return "data-" + base.GetPropName(prop);
        }
    }

    public class ParameterNameAttribute
    {
        public ParameterNameAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; private set; }
    }
}