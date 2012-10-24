
using System.Web.Mvc;

namespace Errordite.Web.Controllers
{
    public class ErrorController : ErrorditeController
    {
        [PreventDirectAccess]
        public ActionResult ServerError()
        {
            return View("ServerError");
        }

        [PreventDirectAccess]
        public ActionResult NotAuthorised()
        {
            return View("NotAuthorised");
        }

        public ActionResult NotFound()
        {
            return View("NotFound");
        }

        [PreventDirectAccess]
        public ActionResult OtherHttpStatusCode(int httpStatusCode)
        {
            return View("ServerError", httpStatusCode);
        }

        private class PreventDirectAccessAttribute : FilterAttribute, IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationContext filterContext)
            {
                object value = filterContext.RouteData.Values["fromAppErrorEvent"];

                if (!(value is bool && (bool)value))
                    filterContext.Result = new ViewResult { ViewName = "NotFound" };
            }
        }
    }
}
