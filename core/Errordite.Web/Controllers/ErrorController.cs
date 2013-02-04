using System.Web.Mvc;
using Errordite.Core.Session;

namespace Errordite.Web.Controllers
{
    public class ErrorController : ErrorditeController
	{
		private readonly IAppSession _session;

		public ErrorController(IAppSession session)
		{
			_session = session;
		}

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
