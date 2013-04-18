using System;
using System.Web.Mvc;
using Errordite.Core.Identity;
using Errordite.Web.Controllers;

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
               appContext.CurrentUser.Organisation.PaymentPlan.IsTrial &&
               appContext.CurrentUser.Organisation.CreatedOnUtc > DateTime.UtcNow.AddDays(-30))
            {
                
            }
        }
    }
}