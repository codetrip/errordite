using System.Linq;
using System.Text;
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
	[RoleAuthorize(UserRole.Administrator)]
    public class SubscriptionController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;
        private readonly ICompleteSignUpCommand _completeSignUpCommand;
	    private readonly ICancelSubscriptionCommand _cancelSubscriptionCommand;
        private readonly IEncryptor _encryptor;
        private readonly IChangeSubscriptionCommand _changeSubscriptionCommand;
		private readonly ISuspendOrganisationCommand _suspendOrganisationCommand;

        public SubscriptionController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, 
			ICompleteSignUpCommand completeSignUpCommand, 
            IEncryptor encryptor, 
			ICancelSubscriptionCommand cancelSubscriptionCommand, 
            IChangeSubscriptionCommand changeSubscriptionCommand,
			ISuspendOrganisationCommand suspendOrganisationCommand)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
            _completeSignUpCommand = completeSignUpCommand;
            _encryptor = encryptor;
	        _cancelSubscriptionCommand = cancelSubscriptionCommand;
            _changeSubscriptionCommand = changeSubscriptionCommand;
	        _suspendOrganisationCommand = suspendOrganisationCommand;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Subscription)]
        public ActionResult Index()
        {
            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;

            var currentPlan = Core.AppContext.CurrentUser.ActiveOrganisation.PaymentPlan;

            var model = new SubscriptionViewModel
            {
                Plans = plans.Select(p => new ChangePaymentPlanViewModel
                {
                    CurrentPlan = p.Id == currentPlan.Id,
                    Upgrade = p.Rank > currentPlan.Rank,
                    Downgrade = p.Rank < currentPlan.Rank,
                    SignUp = currentPlan.IsFreeTier,
                    Plan = p
                }).ToList(),
                Organisation = Core.AppContext.CurrentUser.ActiveOrganisation
            };

            return View(model);
        }

		[HttpGet, Authorize, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.SubscriptionSignUp)]
        public ActionResult SignUp()
        {
			if (!Core.Configuration.SubscriptionsEnabled)
			{
				ConfirmationNotification("Errordite subscriptions are not currently enabled, you may continue using the free trial until subscriptions become active.");
				return RedirectToAction("index");
			}

            var paymentPlans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans.Where(p => !p.IsFreeTier);
		    var model = new SubscriptionSignUpViewModel
		    {
		        Plans = paymentPlans.Select(p => new PaymentPlanViewModel
		        {
		            Plan = p,
		            Status = PaymentPlanStatus.SubscriptionSignUp,
					SignUpUrl = "{0}{1}".FormatWith(p.SignUpUrl, GetSignUpToken(p.FriendlyId)),
					ViewFreeTier = false
		        }),
		    };
            return View(model);
        }

		[HttpGet, Authorize, ExportViewData]
		public ActionResult Complete(SubscriptionCompleteViewModel model)
		{
			if (!Core.Configuration.SubscriptionsEnabled)
			{
				ConfirmationNotification("Errordite subscriptions are not currently enabled, you may continue using the free trial until subscriptions become active.");
				return RedirectToAction("index");
			}

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
			if (!Core.Configuration.SubscriptionsEnabled)
			{
				ConfirmationNotification("Errordite subscriptions are not currently enabled, you may continue using the free trial until subscriptions become active.");
				return RedirectToAction("index");
			}

            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;
	        var newPlan = plans.FirstOrDefault(p => p.FriendlyId == planId.GetFriendlyId());

            if (newPlan == null)
            {
                ErrorNotification("Unrecognised payment plan id {0}, cannot change your subscription.".FormatWith(planId));
                return RedirectToAction("index");
            }

            var model = new ChangeSubscriptionViewModel
            {
                CurrentPlan = Core.AppContext.CurrentUser.ActiveOrganisation.PaymentPlan,
                NewPlan = newPlan,
                NewPlanId = PaymentPlan.GetId(planId),
				CurrentBillingPeriodEnd = Core.AppContext.CurrentUser.ActiveOrganisation.Subscription.CurrentPeriodEndDate,
                Downgrading = newPlan.Rank < Core.AppContext.CurrentUser.ActiveOrganisation.PaymentPlan.Rank
            };

            model.NewPlanName = model.NewPlan.Name;

            return View(model);
        }

        [HttpPost, ExportViewData]
        public ActionResult ChangeSubscription(ChangeSubscriptionPostModel model)
        {
			if (!Core.Configuration.SubscriptionsEnabled)
			{
				ConfirmationNotification("Errordite subscriptions are not currently enabled, you may continue using the free trial until subscriptions become active.");
				return RedirectToAction("index");
			}

            var response = _changeSubscriptionCommand.Invoke(new ChangeSubscriptionRequest
            {
                CurrentUser = Core.AppContext.CurrentUser,
                NewPlanId = model.NewPlanId,
                NewPlanName = model.NewPlanName,
				OldPlanName = model.OldPlanName,
                Downgrading = model.Downgrading
            });

			if (response.Status == ChangeSubscriptionStatus.Ok)
			{
				ConfirmationNotification("Your subscription has been changed successfully.");
				return RedirectToAction("index");
			}

            if (response.Status == ChangeSubscriptionStatus.QuotasExceeded)
            {
                var message = new StringBuilder();
                if (response.Quotas.IssuesExceededBy > 0)
                    message.Append(" Issues exceeded by {0}".FormatWith(response.Quotas.IssuesExceededBy));

                ConfirmationNotification("Cannot downgrade subscription you have exceeded your plan limits.{0}".FormatWith(message.ToString()));
                return RedirectToAction("index");
            }

			return RedirectWithRoute("change", response.Status.MapToResource(Resources.Subscription.ResourceManager), routeValues: new {planId = model.NewPlanId});
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.CancelSubscription)]
        public ActionResult Cancel()
        {
	        var model = new CancelSubscriptionViewModel
		    {
			    Subscription = Core.AppContext.CurrentUser.ActiveOrganisation.Subscription
		    };

			if (ViewData.Model != null)
			{
				model.CancellationReason = ((CancelSubscriptionPostModel) ViewData.Model).CancellationReason;
			}

			return View(model);
        }

		[HttpPost, ExportViewData]
		public ActionResult CancelTrial(CancelSubscriptionPostModel model)
		{
			_suspendOrganisationCommand.Invoke(new SuspendOrganisationRequest
			{
				OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
				Reason = SuspendedReason.SubscriptionCancelled,
				Message = "Trial cancelled by user"
			});

			return Redirect(Url.SignOut());
		}

		[HttpPost, ExportViewData]
		public ActionResult CancelSubscription(CancelSubscriptionPostModel model)
		{
			if (!Core.Configuration.SubscriptionsEnabled)
			{
				ConfirmationNotification("Errordite subscriptions are not currently enabled, you may continue using the free trial until subscriptions become active.");
				return RedirectToAction("index");
			}

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
				Core.AppContext.CurrentUser.ActiveOrganisation.Name,
				HttpUtility.UrlEncode(_encryptor.Encrypt("{0}|{1}".FormatWith(
					Core.AppContext.CurrentUser.ActiveOrganisation.FriendlyId,
					planId)).Base64Encode()));
		}
    }
}
