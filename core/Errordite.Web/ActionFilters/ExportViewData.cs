using System.Web.Mvc;
using Errordite.Web.Extensions;

namespace Errordite.Web.ActionFilters
{
    public class ExportViewData : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //Only export when ModelState is not valid
            if (!filterContext.Controller.ViewData.ModelState.IsValid || filterContext.Controller.ViewData.HasNotification())
            {
                //Export if we are redirecting
                if ((filterContext.Result is RedirectResult) || (filterContext.Result is RedirectToRouteResult))
                {
                    filterContext.Controller.TempData[ImportViewData.ViewDataKey] = filterContext.Controller.ViewData;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}