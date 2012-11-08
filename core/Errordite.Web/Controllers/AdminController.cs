
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Organisation;
using System.Linq;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class AdminController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;
        private readonly ISetOrganisationTimezoneCommand _setOrganisationTimezoneCommand;

        public AdminController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, ISetOrganisationTimezoneCommand setOrganisationTimezoneCommand)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
            _setOrganisationTimezoneCommand = setOrganisationTimezoneCommand;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.PaymentPlan)]
        public ActionResult PaymentPlan()
        {
            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;

            var currentPlan = Core.AppContext.CurrentUser.Organisation.PaymentPlan;

            return View(new OrganisationViewModel
            {
                Plans = plans
                    .Select(p => new PaymentPlanViewModel
                {
                    CurrentPlan = p.Id == currentPlan.Id,
                    Upgrade = p.Rank > currentPlan.Rank,
                    Downgrade = p.Rank < currentPlan.Rank && !p.IsTrial,
                    Plan = p
                }),
            });
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Upgrade)]
        public ActionResult Upgrade()
        {
            return View();
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.OrgSettings)]
        public ActionResult Settings()
        {
            return View(new OrganisationSettingsViewModel
            {
                TimezoneId = Core.AppContext.CurrentUser.Organisation.TimezoneId
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

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Downgrade)]
        public ActionResult Downgrade()
        {
            return View();
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Billing)]
        public ActionResult Billing()
        {
            return View();
        }
    }
}
