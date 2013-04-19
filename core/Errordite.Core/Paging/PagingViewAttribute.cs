using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Errordite.Core.Web;
using ProductionProfiler.Core.Extensions;

namespace Errordite.Core.Paging
{
    public class PagingViewAttribute : ActionFilterAttribute
    {
        public string DefaultSort { get; set; }
        public bool DefaultSortDescending { get; set; }

        private readonly IEnumerable<string> _pagingInfoIds;

        public PagingViewAttribute(params string[] pagingInfoIds)
        {
            _pagingInfoIds = pagingInfoIds;

            if (!_pagingInfoIds.Any())
                _pagingInfoIds = new[] { PagingConstants.DefaultPagingId };
        }

        protected virtual int GetNormalPageSize(IPagingConfiguration pagingConfiguration)
        {
            return pagingConfiguration.NormalPageSize;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var pagingInfoController = filterContext.Controller as BaseController;

            if (pagingInfoController == null)
                return;

            var config = pagingInfoController.PagingConfiguration;

            pagingInfoController.PagingInfos = _pagingInfoIds.ToDictionary(id => id.ToLower(),
                id =>
                new PageRequestWithSort(1, GetNormalPageSize(config), DefaultSort, DefaultSortDescending));

            var queryString = filterContext.RequestContext.HttpContext.Request.QueryString;

            foreach (var queryStringParam in queryString.Cast<string>().Where(t => t != null && t.StartsWith(PagingConstants.QueryStringParameters.PagingPrefix, StringComparison.OrdinalIgnoreCase)))
            {
                string id = ExtractPagingInfoId(queryStringParam);

                if (!pagingInfoController.PagingInfos.ContainsKey(id))
                    pagingInfoController.PagingInfos.Add(id, new PageRequestWithSort(1, GetNormalPageSize(config)));

                var pagingInfo = pagingInfoController.PagingInfos[id];

                if (queryStringParam.StartsWith(PagingConstants.QueryStringParameters.PageNumber))
                {
                    pagingInfo.PageNumber = int.Parse(queryString[queryStringParam]);
                }
                else if (queryStringParam.StartsWith(PagingConstants.QueryStringParameters.PageSize))
                {
                    pagingInfo.PageSize = Math.Min(int.Parse(queryString[queryStringParam]), config.LargePageSize);
                }
                else if (queryStringParam.StartsWith(PagingConstants.QueryStringParameters.PageSortDescending) && queryString[queryStringParam].IsNotNullOrEmpty())
                {
                    pagingInfo.SortDescending = bool.Parse(queryString[queryStringParam]);
                }
                else if (queryStringParam.StartsWith(PagingConstants.QueryStringParameters.PageSort) && queryString[queryStringParam].IsNotNullOrEmpty())
                {
                    pagingInfo.Sort = queryString[queryStringParam];
                }
            }
        }

        private static string ExtractPagingInfoId(string routingKey)
        {
            if (routingKey.Length == 4)
                return PagingConstants.DefaultPagingId;

            return routingKey.Substring(4);
        }
    }
}