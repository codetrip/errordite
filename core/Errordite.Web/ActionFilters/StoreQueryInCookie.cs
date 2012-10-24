using System.Web.Mvc;
using CodeTrip.Core;
using Errordite.Web.Controllers;

namespace Errordite.Web.ActionFilters
{
    public class StoreQueryInCookie : ActionFilterAttribute
    {
        private readonly string _cookieName;

        public StoreQueryInCookie(string cookieName)
        {
            _cookieName = cookieName;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ArgumentValidation.NotEmpty(_cookieName, "_cookieName");

            var controller = filterContext.Controller as ErrorditeController;

            if (controller == null)
                return;
            
            string query = filterContext.RequestContext.HttpContext.Request.Url == null
                ? string.Empty
                : filterContext.RequestContext.HttpContext.Request.Url.Query;

            controller.CookieManager.Set(_cookieName, query, null);

            base.OnActionExecuted(filterContext);
        }
    }
}