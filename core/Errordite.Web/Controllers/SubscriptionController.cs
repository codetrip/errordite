using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Subscription;

namespace Errordite.Web.Controllers
{
    public class SubscriptionController : ErrorditeController
    {
        [HttpGet, Authorize]
        public ActionResult SignUp()
        {
            var model = new SignUpViewModel
            {
                Countries = new List<SelectListItem>(),
                CreditCards = new List<SelectListItem>(),
                Plans = new List<PaymentPlan>()
            };

            return View(model);
        }

        [HttpPost, Authorize]
        public ActionResult SignUp(SignUpViewModel model)
        {
            return View();
        }
    }
}
