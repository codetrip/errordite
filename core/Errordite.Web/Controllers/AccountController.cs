
using System.Web.Mvc;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Account;
using Errordite.Web.Models.Navigation;
using System.Linq;

namespace Errordite.Web.Controllers
{
	[Authorize, ValidateSubscriptionActionFilter]
    public class AccountController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;
        private readonly ISetOrganisationTimezoneCommand _setOrganisationTimezoneCommand;

        public AccountController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, ISetOrganisationTimezoneCommand setOrganisationTimezoneCommand)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
            _setOrganisationTimezoneCommand = setOrganisationTimezoneCommand;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Subscription)]
        public ActionResult Subscription()
        {
            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;

            var currentPlan = Core.AppContext.CurrentUser.Organisation.PaymentPlan;

            var model = new SubscriptionViewModel
            {
                Plans = plans.Select(p => new PaymentPlanViewModel
                {
                    CurrentPlan = p.Id == currentPlan.Id,
                    Upgrade = p.Rank > currentPlan.Rank,
                    Downgrade = p.Rank < currentPlan.Rank && !p.IsTrial,
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

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.BillingHistory)]
		public ActionResult BillingHistory()
		{
			return View();
		}

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.ChangeSubscription)]
		public ActionResult ChangeSubscription()
		{
			return View();
		}

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Cancel)]
		public ActionResult Cancel()
		{
			return View();
		}

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Settings)]
        public ActionResult Settings()
        {
            return View(new OrganisationSettingsViewModel
            {
                TimezoneId = Core.AppContext.CurrentUser.Organisation.TimezoneId,
                ApiKey = Core.AppContext.CurrentUser.Organisation.ApiKey
            });
        }

        [HttpPost, ExportViewData]
        public ActionResult SetTimezone(string timezoneId)
        {
            _setOrganisationTimezoneCommand.Invoke(new SetOrganisationTimezoneRequest
            {
                CurrentUser = AppContext.CurrentUser,
                OrganisationId = AppContext.CurrentUser.OrganisationId,
                TimezoneId = timezoneId
            });

            ConfirmationNotification(Resources.Admin.OrganbisationSettingsUpdated);

            return RedirectToAction("settings");
        }
    }
}
