using System.Linq;
using System.Web.Mvc;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Subscription;

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
            
            return View(paymentPlans.Select(p => new PaymentPlanViewModel
	        {
		        Plan = p,
				Status = PaymentPlanStatus.FirstSignUp
	        }));
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
        public ActionResult Clients(ClientsTab? tab)
        {
            return View(tab.GetValueOrDefault());
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

        [GenerateBreadcrumbs(BreadcrumbId.TermsAndConditions)]
        public ActionResult Terms()
        {
            return View();
        }
    }

    public enum ClientsTab
    {
        DotNet,
        Python,
        Ruby,
        Other
    }
}
