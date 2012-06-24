
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
    public class OrganisationController : ErrorditeController
    {
        private readonly IGetPaymentPlansQuery _getPaymentPlansQuery;
        private readonly ISetOrganisationTimezoneCommand _setOrganisationTimezoneCommand;

        public OrganisationController(IGetPaymentPlansQuery getPaymentPlansQuery, ISetOrganisationTimezoneCommand setOrganisationTimezoneCommand)
        {
            _getPaymentPlansQuery = getPaymentPlansQuery;
            _setOrganisationTimezoneCommand = setOrganisationTimezoneCommand;
        }

        [HttpGet, ImportViewData]
        public ActionResult Index()
        {
            var plans = _getPaymentPlansQuery.Invoke(new GetPaymentPlansRequest()).Plans;

            return View(new OrganisationViewModel
            {
                Plans = plans
                    .Where(p => p.PlanType == PaymentPlanType.Trial || AppContext.CurrentUser.Role == UserRole.SuperUser)
                    .Select(p => new PaymentPlanViewModel
                {
                    CurrentPlan = p.Id == Core.AppContext.CurrentUser.Organisation.PaymentPlanId,
                    Upgrade = p.PlanType > Core.AppContext.CurrentUser.Organisation.PaymentPlan.PlanType && (int)p.PlanType > (int)PaymentPlanType.Small,
                    Downgrade = p.PlanType < Core.AppContext.CurrentUser.Organisation.PaymentPlan.PlanType && p.PlanType != PaymentPlanType.Trial,
                    Plan = p
                }),
            });
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Upgrade)]
        public ActionResult Upgrade()
        {
            return View();
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Settings)]
        public ActionResult Settings()
        {
            return View(new OrganisationSettingsViewModel
            {
                TimezoneId = Core.AppContext.CurrentUser.Organisation.TimezoneId
            });
        }

        [HttpPost]
        public ActionResult SetTimezone(string timezoneId)
        {
            _setOrganisationTimezoneCommand.Invoke(new SetOrganisationTimezoneRequest
            {
                CurrentUser = AppContext.CurrentUser,
                OrganisationId = AppContext.CurrentUser.OrganisationId,
                TimezoneId = timezoneId
            });

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
