using System;
using System.Web.Mvc;
using Errordite.Core.Extensions;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;

namespace Errordite.Web.ActionFilters
{
    public class SelectApplicationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as ErrorditeController;

            if (controller == null)
                return;

            var appId = filterContext.RequestContext.HttpContext.Request.QueryString.Get(WebConstants.RouteValues.SetApplication);

            if (appId != null)
            {
                controller.CookieManager.Set(WebConstants.CookieSettings.ApplicationIdCookieKey, appId, DateTime.UtcNow.AddYears(1));

                var url = new UrlHelper(filterContext.RequestContext).CurrentRequest();

                if (url != null)
                {
                    filterContext.Result = new RedirectResult(url.Replace("{0}={1}".FormatWith(
                        WebConstants.RouteValues.SetApplication,
                        appId), string.Empty));
                }
            }
        }
    }
}