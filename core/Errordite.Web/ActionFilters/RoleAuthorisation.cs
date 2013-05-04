using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Errordite.Core.IoC;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Identity;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;

namespace Errordite.Web.ActionFilters
{
    public class RoleAuthorize : ActionFilterAttribute
    {
        private readonly List<UserRole> _requiredRoles;

        public RoleAuthorize(params UserRole[] requiredRoles)
        {
            _requiredRoles = requiredRoles.ToList();
            _requiredRoles.Add(UserRole.SuperUser);
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

			if (controller.Core.AppContext.CurrentUser == null || !IsUserInRole(controller.Core.AppContext.CurrentUser.Role))
            {
				controller.ErrorNotification("You are not authorised to view the requested page");
				controller.TempData[ImportViewData.ViewDataKey] = controller.ViewData;

				if (controller.Core.AppContext.AuthenticationStatus == AuthenticationStatus.Authenticated)
				{
					var url = new UrlHelper(filterContext.RequestContext);
					filterContext.Result = new RedirectResult(url.Dashboard());
				}
				else
				{
					FormsAuthentication.RedirectToLoginPage();
					filterContext.Result = new EmptyResult();
				}
            }
        }

        private bool IsUserInRole(UserRole currentRole)
        {
            return _requiredRoles.Contains(currentRole);
        }
    }
}