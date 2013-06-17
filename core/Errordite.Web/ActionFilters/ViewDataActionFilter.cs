using System;
using System.Web;
using System.Web.Mvc;
using Errordite.Core.Identity;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Dashboard;

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
			result.ViewData.SetActiveTab(GetActiveTab(controller.Request.Url));
            result.ViewData.SetAppContext(controller.Core.AppContext);
			result.ViewData.SetErrorditeConfiguration(controller.Core.Configuration);
			result.ViewData.SetCore(controller.Core);
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

            controller.ViewData.SetAppContext(controller.Core.AppContext);
            controller.ViewData.SetErrorditeConfiguration(controller.Core.Configuration);
        }

		private static NavTabs GetActiveTab(Uri currentUri)
		{
			if (currentUri == null)
				return NavTabs.None;

			if (currentUri.AbsolutePath.StartsWith("/dashboard/activity"))
				return NavTabs.Activity;

			if (currentUri.AbsolutePath.StartsWith("/dashboard"))
				return NavTabs.Dashboard;

			if (currentUri.AbsolutePath.StartsWith("/issues/add"))
				return NavTabs.AddIssue;

			if (currentUri.AbsolutePath.StartsWith("/errors"))
				return NavTabs.Errors;

			if (currentUri.AbsolutePath.StartsWith("/issue"))
				return NavTabs.Issues;

			if (currentUri.AbsolutePath.StartsWith("/docs"))
				return NavTabs.Docs;

			if (currentUri.AbsolutePath.StartsWith("/test"))
				return NavTabs.Test;

			if (currentUri.AbsolutePath.StartsWith("/contact"))
				return NavTabs.Contact;

			if (currentUri.AbsolutePath.StartsWith("/search"))
				return NavTabs.None;

			return NavTabs.Account;
		}
    }
}