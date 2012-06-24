using System.Web.Mvc;

namespace Errordite.Web.ActionFilters
{
    public class ImportViewData : ActionFilterAttribute
    {
        public const string ViewDataKey = "importexportviewdata";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var viewData = filterContext.Controller.TempData[ViewDataKey] as ViewDataDictionary;

            if (viewData != null)
            {
                filterContext.Controller.ViewData = viewData;
            }

            filterContext.Controller.TempData.Remove(ViewDataKey);
            base.OnActionExecuting(filterContext);
        }
    }
}