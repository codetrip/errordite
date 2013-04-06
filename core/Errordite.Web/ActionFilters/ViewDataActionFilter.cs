using System;
using System.Web.Mvc;
using Errordite.Core.Identity;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;
using ProductionProfiler.Core.Extensions;

namespace Errordite.Web.ActionFilters
{
    public class ViewDataActionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Used to ensure that <see cref="AppContext"/> is updates before rendering a new page.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var result = filterContext.Result as ViewResult;
            if (result == null)
                return;

            var controller = filterContext.Controller as ErrorditeController;
            if (controller == null)
                return;

            result.ViewData.SetCookieManager(controller.CookieManager);
            result.ViewData.SetCore(controller.Core);
            result.ViewData.SetAppContext(controller.Core.AppContext);
            result.ViewData.SetErrorditeConfiguration(controller.Core.Configuration);
        }

        /// <summary>
        /// Used to ensure that <see cref="AppContext"/> is set when action crashes before rendering.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as ErrorditeController;

            if (controller == null)
                return;

            var appId = filterContext.RequestContext.HttpContext.Request.QueryString.Get(WebConstants.RouteValues.SetApplication);

            if (appId != null)
            {
                controller.CookieManager.Set(WebConstants.CookieSettings.ApplicationIdCookieKey, appId, DateTime.UtcNow.AddYears(1));

                var url = filterContext.RequestContext.HttpContext.Request.Url;

                if (url != null)
                {
                    filterContext.Result = new RedirectResult(url.AbsoluteUri.Replace("{0}={1}".FormatWith(
                        WebConstants.RouteValues.SetApplication,
                        appId), string.Empty));
                }
            }

            controller.ViewData.SetAppContext(controller.Core.AppContext);
            controller.ViewData.SetErrorditeConfiguration(controller.Core.Configuration);
        }
    }
}