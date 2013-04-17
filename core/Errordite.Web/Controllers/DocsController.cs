using System.Linq;
using System.Web.Mvc;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;

namespace Errordite.Web.Controllers
{
    public class DocsController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;

        public DocsController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
        }

        [GenerateBreadcrumbs(BreadcrumbId.Pricing)]
        public ActionResult Pricing()
        {
            var paymentPlans =
                _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans.Where(p => !p.IsTrial);
            
            return View(paymentPlans);
        }

        [GenerateBreadcrumbs(BreadcrumbId.QuickStart)]
        public ActionResult QuickStart()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Features)]
        public ActionResult API()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Clients)]
        public ActionResult Clients()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Privacy)]
        public ActionResult Privacy()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.About)]
        public ActionResult About()
        {
            return View();
        }
    }
}
