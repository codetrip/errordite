using System;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Identity;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;

namespace Errordite.Web.ActionFilters
{
    public class ValidateSubscriptionActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as ErrorditeController;

            if (controller == null)
                return;

            var appContext = controller.Core.AppContext;

            if(appContext.AuthenticationStatus == AuthenticationStatus.Authenticated &&
			   controller.Core.Configuration.SubscriptionsEnabled &&
			   appContext.CurrentUser.Organisation.Subscription.Status == SubscriptionStatus.Trial &&
               appContext.CurrentUser.Organisation.Subscription.CurrentPeriodEndDate.Date <= DateTime.UtcNow.Date)
            {
                filterContext.HttpContext.Response.Redirect(new UrlHelper(filterContext.RequestContext).SignUpExpired());
                filterContext.Result = new EmptyResult();
            }
        }
    }
}