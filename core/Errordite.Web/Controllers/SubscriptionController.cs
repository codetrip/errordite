using System.Linq;
using System.Web;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
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
	    private readonly ICancelSubscriptionCommand _cancelSubscriptionCommand;
        private readonly IEncryptor _encryptor;
        private readonly IChangeSubscriptionCommand _changeSubscriptionCommand;

        public SubscriptionController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, 
			ICompleteSignUpCommand completeSignUpCommand, 
            IEncryptor encryptor, 
			ICancelSubscriptionCommand cancelSubscriptionCommand, 
            IChangeSubscriptionCommand changeSubscriptionCommand)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
            _completeSignUpCommand = completeSignUpCommand;
            _encryptor = encryptor;
	        _cancelSubscriptionCommand = cancelSubscriptionCommand;
            _changeSubscriptionCommand = changeSubscriptionCommand;
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

		[HttpGet, Authorize, GenerateBreadcrumbs(BreadcrumbId.SubscriptionSignUp)]
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

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.SubscriptionSignUpFailed)]
        public ActionResult Failed(SubscriptionCompleteViewModel model)
        {
            return View(model);
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.BillingHistory)]
        public ActionResult BillingHistory()
        {
            return View();
        }

        [HttpGet, ImportViewData, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.ChangeSubscription)]
        public ActionResult Change(string planId)
        {
            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;
            var model = new ChangeSubscriptionViewModel
            {
                CurrentPlan = Core.AppContext.CurrentUser.Organisation.PaymentPlan,
                NewPlan = plans.FirstOrDefault(p => p.FriendlyId == planId.GetFriendlyId()),
                NewPlanId = PaymentPlan.GetId(planId),
				CurrentBillingPeriodEnd = Core.AppContext.CurrentUser.Organisation.Subscription.CurrentPeriodEndDate.Value
            };

            if (model.NewPlan == null)
            {
                ErrorNotification("Unrecognised payment plan id {0}, cannot change your subscription.".FormatWith(planId));
                return RedirectToAction("index");
            }

            model.NewPlanName = model.NewPlan.Name;

            return View(model);
        }

        [HttpPost, ExportViewData]
        public ActionResult ChangeSubscription(ChangeSubscriptionPostModel model)
        {
            var response = _changeSubscriptionCommand.Invoke(new ChangeSubscriptionRequest
            {
                CurrentUser = Core.AppContext.CurrentUser,
                NewPlanId = model.NewPlanId,
                NewPlanName = model.NewPlanName,
				OldPlanName = model.OldPlanName
            });

			if (response.Status == ChangeSubscriptionStatus.Ok)
			{
				ConfirmationNotification("Your subscription has been changed successfully.");
				return RedirectToAction("index");
			}

			return RedirectWithRoute("change", response.Status.MapToResource(Resources.Subscription.ResourceManager), routeValues: new {planId = model.NewPlanId});
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.CancelSubscription)]
        public ActionResult Cancel()
        {
	        var model = new CancelSubscriptionViewModel
		    {
			    Subscription = Core.AppContext.CurrentUser.Organisation.Subscription
		    };

			if (ViewData.Model != null)
			{
				model.CancellationReason = ((CancelSubscriptionPostModel) ViewData.Model).CancellationReason;
			}

			return View(model);
        }

		[HttpPost, ExportViewData]
		public ActionResult CancelSubscription(CancelSubscriptionPostModel model)
		{
			var response = _cancelSubscriptionCommand.Invoke(new CancelSubscriptionRequest
			{
				CurrentUser = Core.AppContext.CurrentUser,
				CancellationReason = model.CancellationReason
			});

			if (response.Status == CancelSubscriptionStatus.Ok)
			{
				ConfirmationNotification("Your subscription has been cancelled, your account will become inactive on {0}.".FormatWith(response.AccountExpirationDate.ToLocalFormatted()));
				return RedirectToAction("index");
			}

			return RedirectWithViewModel(model, "cancel", response.Status.MapToResource(Resources.Subscription.ResourceManager));
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
