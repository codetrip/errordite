using System.Web.Mvc;
using Errordite.Core.Organisations.Commands;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Subscription;

namespace Errordite.Web.Controllers
{
	[Authorize, ValidateSubscriptionActionFilter]
    public class AccountController : ErrorditeController
    {
        private readonly ISetOrganisationTimezoneCommand _setOrganisationTimezoneCommand;

        public AccountController(ISetOrganisationTimezoneCommand setOrganisationTimezoneCommand)
        {
            _setOrganisationTimezoneCommand = setOrganisationTimezoneCommand;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Settings)]
        public ActionResult Timezone()
        {
            return View(new OrganisationSettingsViewModel
            {
                TimezoneId = Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId,
                ApiKey = Core.AppContext.CurrentUser.ActiveOrganisation.ApiKey
            });
        }

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Settings)]
		public ActionResult ApiAccess()
		{
			return View(new OrganisationSettingsViewModel
			{
				TimezoneId = Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId,
				ApiKey = Core.AppContext.CurrentUser.ActiveOrganisation.ApiKey
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

            return RedirectToAction("timezone");
        }
    }
}
