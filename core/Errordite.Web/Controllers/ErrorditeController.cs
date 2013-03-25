using System;
using System.IO;
using System.Resources;
using System.Web.Mvc;
using CodeTrip.Core.Dynamic;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Web;
using Errordite.Core;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Identity;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Notifications;
using CodeTrip.Core.Extensions;
using System.Linq;

namespace Errordite.Web.Controllers
{
    public abstract class ErrorditeController : BaseController
    {
        //castle cant statically resolve all dependencies on IErrorditeCore hance the implementation below
        private IErrorditeCore _errorditeCore;
        public IErrorditeCore Core
        {
            get { return _errorditeCore ?? (_errorditeCore = ObjectFactory.GetObject<IErrorditeCore>()); }
        }

        public ICookieManager CookieManager { get; set; }
        public AppContext AppContext { protected get; set; }

        protected Application CurrentApplication
        {
            get
            {
                var appId = CookieManager.Get(WebConstants.CookieSettings.ApplicationIdCookieKey);

                if (appId.IsNotNullOrEmpty())
                    return Core.GetApplications().Items.FirstOrDefault(a => a.FriendlyId == appId) ?? new Application();

                return new Application();
            }
        }

        protected ActionResult RedirectWithViewModel<TPostModel, TViewModel>(TPostModel postModel, string action, string message = null, bool error = true, object routeValues = null) where TViewModel : class, new()
        {
            var viewModel = new TViewModel();
            PropertyMapper<TPostModel, TViewModel>.Map(postModel, viewModel);

            ViewData.Model = viewModel;

            if (!message.IsNullOrEmpty())
                ViewData.SetNotification(error ? UiNotification.Error(message) : UiNotification.Confirmation(message));

            return routeValues == null ? RedirectToAction(action) : RedirectToAction(action, routeValues);
        }

        protected ActionResult RedirectWithViewModel<T>(T viewModel, string action, string message = null, bool error = true, object routeValues = null)
        {
            ViewData.Model = viewModel;

            if (!message.IsNullOrEmpty())
                ViewData.SetNotification(error ? UiNotification.Error(message) : UiNotification.Confirmation(message));

            return routeValues == null ? RedirectToAction(action) : RedirectToAction(action, routeValues);
        }

        protected void SetNotification(Enum status, ResourceManager rm)
        {
            if (status.ToString().ToLowerInvariant() == "ok")
                ConfirmationNotification(status.MapToResource(rm));
            else
                ErrorNotification(status.MapToResource(rm));
        }

        protected void ConfirmationNotification(MvcHtmlString message)
        {
            ViewData.SetNotification(UiNotification.Confirmation(message));
        }

        protected void ConfirmationNotification(string unencodedMessage)
        {
            ViewData.SetNotification(UiNotification.Confirmation(unencodedMessage));
        }

        protected void InfoNotification(MvcHtmlString unencodedMessage)
        {
            ViewData.SetNotification(UiNotification.Info(unencodedMessage));
        }

        protected void InfoNotification(string message)
        {
            ViewData.SetNotification(UiNotification.Info(message));
        }

        protected void ErrorNotification(MvcHtmlString message)
        {
            ViewData.SetNotification(UiNotification.Error(message));
        }

        protected void ErrorNotification(string unencodedMessage)
        {
            ViewData.SetNotification(UiNotification.Error(unencodedMessage));
        }

        protected override void OnException(ExceptionContext exceptionContext)
        {
            var user = (Core == null || Core.AppContext == null ? null : Core.AppContext.CurrentUser);

            if (user != null)
                exceptionContext.Exception.Data.Add(CoreConstants.ExceptionKeys.User, "{0} ({1})".FormatWith(user.FullName, user.Id));
            
            Error(exceptionContext.Exception);
        }

		public string RenderPartial(string partialPath, object model)
		{
			if (string.IsNullOrEmpty(partialPath))
				partialPath = ControllerContext.RouteData.GetRequiredString("action");

			ViewData.Model = model;

			using (var sw = new StringWriter())
			{
				var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, partialPath);
				var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);

				// copy model state items to the html helper 
				foreach (var item in viewContext.Controller.ViewData.ModelState)
				{
					if (!viewContext.ViewData.ModelState.Keys.Contains(item.Key))
					{
						viewContext.ViewData.ModelState.Add(item);
					}
				}

				viewResult.View.Render(viewContext, sw);

				return sw.GetStringBuilder().ToString();
			}
		}
    }
}
