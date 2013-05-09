using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Commands;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Account;
using Errordite.Web.Models.Navigation;

namespace Errordite.Web.Controllers
{
	[Authorize, ValidateSubscriptionActionFilter]
    public class AccountController : ErrorditeController
    {
        private readonly ISetOrganisationTimezoneCommand _setOrganisationTimezoneCommand;
		private readonly ISetOrganisationHipChatSettingsCommand _setOrganisationHipChatSettingsCommand;
		private readonly ISetOrganisationCampfireSettingsCommand _setOrganisationCampfireSettingsCommand;

        public AccountController(ISetOrganisationTimezoneCommand setOrganisationTimezoneCommand, 
			ISetOrganisationHipChatSettingsCommand setOrganisationHipChatSettingsCommand, 
			ISetOrganisationCampfireSettingsCommand setOrganisationCampfireSettingsCommand)
        {
	        _setOrganisationTimezoneCommand = setOrganisationTimezoneCommand;
	        _setOrganisationHipChatSettingsCommand = setOrganisationHipChatSettingsCommand;
	        _setOrganisationCampfireSettingsCommand = setOrganisationCampfireSettingsCommand;
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
		public ActionResult Campfire()
		{
			var campfireDetails = Core.AppContext.CurrentUser.ActiveOrganisation.CampfireDetails ?? new CampfireDetails();

			return View(new CampfireSettingsViewModel
			{
				CampfireCompany = campfireDetails.Company,
				CampfireToken = campfireDetails.Token,
			});
		}

		[HttpPost, ExportViewData]
		public ActionResult SetCampfire(CampfireSettingsViewModel model)
		{
			if (!ModelState.IsValid)
				return RedirectWithViewModel(model, "campfire");

			_setOrganisationCampfireSettingsCommand.Invoke(new SetOrganisationCampfireSettingsRequest
			{
				CurrentUser = Core.AppContext.CurrentUser,
				OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
				CampfireCompany = model.CampfireCompany,
				CampfireToken = model.CampfireToken,
			});

			ConfirmationNotification(Resources.Account.CampfireSettingsUpdated);
			return RedirectToAction("campfire");
		}

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Settings)]
		public ActionResult HipChat()
		{
			return View(new HipChatSettingsViewModel
			{
				HipChatAuthToken = Core.AppContext.CurrentUser.ActiveOrganisation.HipChatAuthToken
			});
		}

		[HttpPost, ExportViewData]
		public ActionResult SetHipChat(HipChatSettingsViewModel model)
		{
			if (!ModelState.IsValid)
				return RedirectWithViewModel(model, "hipchat");

			_setOrganisationHipChatSettingsCommand.Invoke(new SetOrganisationHipChatSettingsRequest
			{
				CurrentUser = Core.AppContext.CurrentUser,
				OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
				HipChatAuthToken = model.HipChatAuthToken
			});

			ConfirmationNotification(Resources.Account.HipChatSettingsUpdated);
			return RedirectToAction("hipchat");
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

            ConfirmationNotification(Resources.Admin.OrganisationSettingsUpdated);

            return RedirectToAction("timezone");
        }
    }
}
