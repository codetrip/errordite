using System;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Commands;
using Errordite.Web.ActionFilters;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Account;
using Errordite.Web.Models.Navigation;
using System.Linq;

namespace Errordite.Web.Controllers
{
	[Authorize]
    public class AccountController : ErrorditeController
    {
        private readonly IUpdateOrganisationCommand _updateOrganisationCommand;
		private readonly ISetOrganisationHipChatSettingsCommand _setOrganisationHipChatSettingsCommand;
		private readonly ISetOrganisationCampfireSettingsCommand _setOrganisationCampfireSettingsCommand;
		//private readonly IAddReplayReplacementCommand _addReplayReplacementCommand;
		//private readonly IDeleteReplayReplacementCommand _deleteReplayReplacementCommand;

        public AccountController(IUpdateOrganisationCommand updateOrganisationCommand, 
			ISetOrganisationHipChatSettingsCommand setOrganisationHipChatSettingsCommand, 
			ISetOrganisationCampfireSettingsCommand setOrganisationCampfireSettingsCommand)
			//IDeleteReplayReplacementCommand deleteReplayReplacementCommand, 
			//IAddReplayReplacementCommand addReplayReplacementCommand)
        {
	        _updateOrganisationCommand = updateOrganisationCommand;
	        _setOrganisationHipChatSettingsCommand = setOrganisationHipChatSettingsCommand;
	        _setOrganisationCampfireSettingsCommand = setOrganisationCampfireSettingsCommand;
			//_deleteReplayReplacementCommand = deleteReplayReplacementCommand;
			//_addReplayReplacementCommand = addReplayReplacementCommand;
        }

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Settings)]
        public ActionResult Organisation()
		{
		    var users = Core.GetUsers();
		    var primaryUser =
		        users.Items.First(u => u.Id.ToLowerInvariant() == Core.AppContext.CurrentUser.ActiveOrganisation.PrimaryUserId.ToLowerInvariant());

		    var model = new OrganisationSettingsViewModel
		    {
		        TimezoneId = Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId,
		        OrganisationName = Core.AppContext.CurrentUser.ActiveOrganisation.Name,
		        Users = users
		            .Items
		            .ToSelectList(u => u.FriendlyId, u => u.FullName, u => u.Id == primaryUser.Id, sortListBy: SortSelectListBy.Text),
		    };

            return View(model);
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

		//[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Settings)]
		//public ActionResult ReplayReplacements()
		//{
		//	return View(new ReplayReplacementsViewModel
		//	{
		//		ReplayReplacements = Core.AppContext.CurrentUser.ActiveOrganisation.ReplayReplacements
		//	});
		//}

		//[HttpPost, ExportViewData]
		//public ActionResult AddReplayReplacement(ReplayReplacementsPostModel model)
		//{
		//	if (!ModelState.IsValid)
		//		return RedirectWithViewModel(model, "replayreplacements");

		//	_addReplayReplacementCommand.Invoke(new AddReplayReplacementRequest
		//	{
		//		CurrentUser = Core.AppContext.CurrentUser,
		//		OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
		//		Field = model.Field,
		//		Find = model.Find,
		//		Replace = model.Replace
		//	});

		//	ConfirmationNotification("Replay replacement added successfully");
		//	return RedirectToAction("replayreplacements");
		//}

		//[HttpPost, ExportViewData]
		//public ActionResult DeleteReplayReplacement(Guid id)
		//{
		//	_deleteReplayReplacementCommand.Invoke(new DeleteReplayReplacementRequest
		//	{
		//		CurrentUser = Core.AppContext.CurrentUser,
		//		OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
		//		Id = id
		//	});

		//	ConfirmationNotification("Replay replacement deleted successfully");
		//	return RedirectToAction("replayreplacements");
		//}

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
		public ActionResult UpdateOrganisation(OrganisationSettingsPostModel model)
        {
            _updateOrganisationCommand.Invoke(new UpdateOrganisationRequest
            {
                CurrentUser = AppContext.CurrentUser,
                OrganisationId = AppContext.CurrentUser.OrganisationId,
                TimezoneId = model.TimezoneId,
                Name = model.OrganisationName,
                PrimaryUserId = model.PrimaryUserId
            });

            ConfirmationNotification(Resources.Admin.OrganisationSettingsUpdated);

            return RedirectToAction("organisation");
        }
    }
}
