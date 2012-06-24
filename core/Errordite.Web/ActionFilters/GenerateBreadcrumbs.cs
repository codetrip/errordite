using System.Linq;
using System.Web.Mvc;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Navigation;
using CodeTrip.Core.Extensions;

namespace Errordite.Web.ActionFilters
{
    public class GenerateBreadcrumbs : ActionFilterAttribute
    {
        private readonly BreadcrumbId _breadcrumbId;
        private readonly BreadcrumbId? _overrideBreadcrumbId;
        private readonly string _cookieKey;

        public GenerateBreadcrumbs(BreadcrumbId breadcrumbId)
        {
            _breadcrumbId = breadcrumbId;
        }

        public GenerateBreadcrumbs(BreadcrumbId breadcrumbId, BreadcrumbId overrideBreadcrumb, string cookieKey)
        {
            _breadcrumbId = breadcrumbId;
            _overrideBreadcrumbId = overrideBreadcrumb;
            _cookieKey = cookieKey;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as ErrorditeController;

            if (controller == null)
                return;

            var breadcrumbs = Breadcrumbs.GetBreadcrumbsForRoute(_breadcrumbId, controller.Url);

            if (breadcrumbs != null)
            {
                if (_overrideBreadcrumbId.HasValue && _cookieKey.IsNotNullOrEmpty())
                {
                    var value = controller.CookieManager.Get(_cookieKey);

                    if(value.IsNotNullOrEmpty())
                    {
                        var overriddenCrumbs = breadcrumbs.Select(breadcrumb => breadcrumb.Id == _overrideBreadcrumbId.Value ?
                            new Breadcrumb(breadcrumb.Id, breadcrumb.Link + value, breadcrumb.Title) : 
                            breadcrumb).ToList();

                        controller.ViewData.SetBreadcrumbs(overriddenCrumbs);
                    }
                    else
                    {
                        controller.ViewData.SetBreadcrumbs(breadcrumbs);
                    }
                    
                }
                else
                {
                    controller.ViewData.SetBreadcrumbs(breadcrumbs);
                }
            }
        }
    }
}