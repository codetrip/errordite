using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
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
        public ActionResult SignUp(string planName)
        {
			var paymentPlans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans.Where(p => !p.IsTrial);
	        var selectedPlan = paymentPlans.FirstOrDefault(p => p.Name.ToLowerInvariant() == planName.ToLowerInvariant());

			if (selectedPlan == null)
			{
				return RedirectToAction("trialexpired");
			}
            var model = new SignUpViewModel
            {
                Countries = new List<SelectListItem>(),
                CreditCards = new List<SelectListItem>(),
                SelectedPlan = new PaymentPlanViewModel
	                {
						Plan = selectedPlan,
						Status = PaymentPlanStatus.SelectedPlan
	                } 
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult SignUp(SignUpViewModel model)
        {
            return View();
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
