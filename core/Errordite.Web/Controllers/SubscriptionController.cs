using System.Linq;
using System.Web;
using System.Web.Mvc;
using Errordite.Core.Encryption;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Subscription;
using Errordite.Core.Extensions;
using Errordite.Web.Extensions;

namespace Errordite.Web.Controllers
{
    public class SubscriptionController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;
        private readonly ICompleteSignUpCommand _completeSignUpCommand;
        private readonly IEncryptor _encryptor;

        public SubscriptionController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, 
			ICompleteSignUpCommand completeSignUpCommand, 
            IEncryptor encryptor)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
            _completeSignUpCommand = completeSignUpCommand;
            _encryptor = encryptor;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Subscription)]
        public ActionResult Index()
        {
            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;

            var currentPlan = Core.AppContext.CurrentUser.Organisation.PaymentPlan;

            var model = new SubscriptionViewModel
            {
                Plans = plans.Select(p => new ChangePaymentPlanViewModel
                {
                    CurrentPlan = p.Id == currentPlan.Id,
                    Upgrade = p.Rank > currentPlan.Rank && !currentPlan.IsTrial,
                    Downgrade = p.Rank < currentPlan.Rank && !p.IsTrial,
                    SignUp = currentPlan.IsTrial && !p.IsTrial,
                    Plan = p
                }).ToList(),
                Organisation = Core.AppContext.CurrentUser.Organisation
            };

            if (!model.Organisation.PaymentPlan.IsTrial)
            {
                model.Plans.Remove(model.Plans.First(p => p.Plan.IsTrial));
            }

            return View(model);
        }

        [HttpGet, Authorize]
        public ActionResult SignUp(bool expired = false)
        {
            var paymentPlans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans.Where(p => !p.IsTrial);
		    var model = new SubscriptionSignUpViewModel
		    {
		        Plans = paymentPlans.Select(p => new PaymentPlanViewModel
		        {
		            Plan = p,
		            Status = PaymentPlanStatus.SubscriptionSignUp,
		            SignUpUrl = "{0}{1}".FormatWith(p.SignUpUrl, GetSignUpToken(p.FriendlyId))
		        }),
		        Expired = expired
		    };
            return View(model);
        }

		[HttpGet, Authorize, ExportViewData]
		public ActionResult Complete(SubscriptionCompleteViewModel model)
		{
		    var status = _completeSignUpCommand.Invoke(new CompleteSignUpRequest
		    {
		        CurrentUser = Core.AppContext.CurrentUser,
		        Reference = model.Reference,
		        SubscriptionId = model.SubscriptionId
		    }).Status;

            if (status == CompleteSignUpStatus.Ok)
            {
                ConfirmationNotification("Your subscription has been created successfully, thank you.");
                return RedirectToAction("index");
            }

            return RedirectToAction("failed", new { SubscritpionId = model.SubscriptionId});
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Subscription)]
        public ActionResult Failed(SubscriptionCompleteViewModel model)
        {
            return View(model);
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.BillingHistory)]
        public ActionResult BillingHistory()
        {
            return View();
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.ChangeSubscription)]
        public ActionResult ChangeSubscription(string planId)
        {
            return View();
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.CancelSubscription)]
        public ActionResult Cancel()
        {
            return View();
        }

		public ActionResult ChargifyComplete(SubscriptionCompleteViewModel model)
		{
			return Redirect(Url.Complete(model.SubscriptionId, model.Reference));
		}

		private string GetSignUpToken(string planId)
		{
			return "?first_name={0}&last_name={1}&email={2}&organisation={3}&reference={4}".FormatWith(
				Core.AppContext.CurrentUser.FirstName,
				Core.AppContext.CurrentUser.LastName,
				Core.AppContext.CurrentUser.Email,
				Core.AppContext.CurrentUser.Organisation.Name,
				HttpUtility.UrlEncode(_encryptor.Encrypt("{0}|{1}".FormatWith(
					Core.AppContext.CurrentUser.Organisation.FriendlyId,
					planId)).Base64Encode()));
		}
    }
}
