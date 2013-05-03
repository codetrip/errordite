using System;
using System.Web.Mvc;
using Errordite.Core;
using Errordite.Core.Extensions;
using Errordite.Core.IoC;
using Errordite.Core.Web;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;

namespace Errordite.Web.ActionFilters
{
    public class SelectApplicationOrOrganisationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as ErrorditeController;

            if (controller == null)
                return;

			var cookieManager = controller.CookieManager;

	        if (cookieManager == null)
				cookieManager = ObjectFactory.GetObject<ICookieManager>();

            var appId = filterContext.RequestContext.HttpContext.Request.QueryString.Get(WebConstants.RouteValues.SetApplication);

            if (appId != null)
            {
				cookieManager.Set(WebConstants.CookieSettings.ApplicationIdCookieKey, appId, DateTime.UtcNow.AddYears(1));

                var url = new UrlHelper(filterContext.RequestContext).CurrentRequest();

                if (url != null)
                {
                    filterContext.Result = new RedirectResult(url.Replace("{0}={1}".FormatWith(
                        WebConstants.RouteValues.SetApplication,
                        appId), string.Empty));
                }
            }

			var orgId = filterContext.RequestContext.HttpContext.Request.QueryString.Get(WebConstants.RouteValues.SetOrganisation);

			if (orgId != null)
			{
				cookieManager.Set(CoreConstants.OrganisationIdCookieKey, orgId, DateTime.UtcNow.AddYears(1));

				var url = new UrlHelper(filterContext.RequestContext).CurrentRequest();

				if (url != null)
				{
					filterContext.Result = new RedirectResult(url.Replace("{0}={1}".FormatWith(
						WebConstants.RouteValues.SetOrganisation,
						orgId), string.Empty));
				}
			}
        }
    }
}