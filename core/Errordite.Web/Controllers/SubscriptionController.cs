using System.Linq;
using System.Web.Mvc;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.Models.Subscription;

namespace Errordite.Web.Controllers
{
	[Authorize]
    public class SubscriptionController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;

        public SubscriptionController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
        }

        [HttpGet]
        public ActionResult TrialExpired()
        {
            var paymentPlans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans.Where(p => !p.IsTrial);
            return View(paymentPlans.Select(p => new PaymentPlanViewModel
	            {
		            Plan = p,
					Status = PaymentPlanStatus.SubscriptionSignUp
	            }));
        }
    }
}
