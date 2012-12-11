using System.Linq;
using System.Web.Mvc;
using Errordite.Core.Session;
using Errordite.Web.Models.Errors;

namespace Errordite.Web.Controllers
{
    public class ErrorController : ErrorditeController
	{
		private readonly IAppSession _session;

		public ErrorController(IAppSession session)
		{
			_session = session;
		}

	    public ActionResult Index(string id)
	    {
		    var viewModel = new ErrorInstanceViewModel();
			var applications = Core.GetApplications();

			var error = _session.Raven.Load<Core.Domain.Error.Error>(Errordite.Core.Domain.Error.Error.GetId(id));

			if (error != null)
			{
				viewModel.Error =  error;
				viewModel.AutoOpen = true;
				viewModel.ApplicationName = applications.Items.First(a => a.Id == error.ApplicationId).Name;
			}

			return View(viewModel);
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
