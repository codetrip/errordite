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
            var model = new SignUpViewModel
            {
                Countries = new List<SelectListItem>(),
                CreditCards = new List<SelectListItem>(),
                Plans = new List<PaymentPlan>()
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
            return View(paymentPlans);
        }
    }
}
