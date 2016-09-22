using System;
using System.Web.Mvc;
using Errordite.Core;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Identity;
using Errordite.Core.IoC;
using Errordite.Core.Session;
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
            
			var cookieManager = controller.CookieManager ?? ObjectFactory.GetObject<ICookieManager>();

	        if (CheckExplicitRequestToChange(filterContext, cookieManager)) 
                return;

            CheckRequestImplicitInUrl(filterContext, cookieManager);
        }

        private static void CheckRequestImplicitInUrl(ActionExecutingContext filterContext, ICookieManager cookieManager)
        {
            var orgid = filterContext.RequestContext.RouteData.Values["orgid"].IfPoss(v => v.ToString());
            var appid = filterContext.RequestContext.RouteData.Values["appid"].IfPoss(v => v.ToString());

            if (orgid != null)
            {
                var appContextFactory = ObjectFactory.GetObject<IAppContextFactory>();
                if (!appContextFactory.TryChangeOrg(orgid))
                    throw new ErrorditeAuthorisationException(new Organisation() {Id = Organisation.GetId(orgid)},
                        appContextFactory.Create().CurrentUser);
            }

            if (appid != null)
            {
                cookieManager.Set(WebConstants.CookieSettings.ApplicationIdCookieKey, appid, DateTime.UtcNow.AddYears(1));
            }
        }

        private static bool CheckExplicitRequestToChange(ActionExecutingContext filterContext, ICookieManager cookieManager)
        {
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
                    return true;
                }
            }

            var orgId =
                filterContext.RequestContext.HttpContext.Request.QueryString.Get(WebConstants.RouteValues.SetOrganisation);

            if (orgId != null)
            {
                cookieManager.Set(CoreConstants.OrganisationIdCookieKey, orgId, DateTime.UtcNow.AddYears(1));
                filterContext.Result = new RedirectResult(new UrlHelper(filterContext.RequestContext).Dashboard());
                return true;
            }
            return false;
        }
    }
}