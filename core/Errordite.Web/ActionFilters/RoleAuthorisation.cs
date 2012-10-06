using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CodeTrip.Core.IoC;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Identity;

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
            AppContext appContext = ObjectFactory.GetObject<AppContext>();

            if (appContext.ImpersonatorAppContext != null)
                return;

            if (appContext.CurrentUser == null || !IsUserInRole(appContext.CurrentUser.Role))
            {
                FormsAuthentication.RedirectToLoginPage();
                filterContext.Result = new EmptyResult();
            }
        }

        private bool IsUserInRole(UserRole currentRole)
        {
            return _requiredRoles.Contains(currentRole);
        }
    }
}