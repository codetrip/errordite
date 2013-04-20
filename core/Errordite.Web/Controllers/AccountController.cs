
using System.Web.Mvc;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Account;
using Errordite.Web.Models.Navigation;
using System.Linq;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class AccountController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;
        private readonly ISetOrganisationTimezoneCommand _setOrganisationTimezoneCommand;

        public AccountController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, ISetOrganisationTimezoneCommand setOrganisationTimezoneCommand)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
            _setOrganisationTimezoneCommand = setOrganisationTimezoneCommand;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Billing)]
        public ActionResult Billing()
        {
            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;

            var currentPlan = Core.AppContext.CurrentUser.Organisation.PaymentPlan;

            return View(new OrganisationViewModel
            {
                Plans = plans.Select(p => new PaymentPlanViewModel
                {
                    CurrentPlan = p.Id == currentPlan.Id,
                    Upgrade = p.Rank > currentPlan.Rank,
                    Downgrade = p.Rank < currentPlan.Rank && !p.IsTrial,
                    Plan = p
                }),
            });
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
