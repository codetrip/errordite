﻿using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Users.Commands;
using Errordite.Core.Users.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Groups;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Users;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class UsersController : ErrorditeController
    {
        private readonly IGetUserQuery _getUserQuery;
        private readonly IEditUserCommand _editUserCommand;
        private readonly IAddUserCommand _addUserCommand;
        private readonly IDeleteUserCommand _deleteUserCommand;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
		private readonly ISendNotificationCommand _sendNotificationCommand;
		private readonly IEncryptor _encryptor;

        public UsersController(IAddUserCommand addUserCommand, 
            IEditUserCommand editUserCommand, 
            IDeleteUserCommand deleteUserCommand, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IGetUserQuery getUserQuery, 
			ISendNotificationCommand sendNotificationCommand, 
			IEncryptor encryptor)
        {
            _addUserCommand = addUserCommand;
            _editUserCommand = editUserCommand;
            _deleteUserCommand = deleteUserCommand;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _getUserQuery = getUserQuery;
	        _sendNotificationCommand = sendNotificationCommand;
	        _encryptor = encryptor;
        }

        [PagingView, HttpGet, RoleAuthorize(UserRole.Administrator), ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Users)]
        public ActionResult Index(string groupId)
        {
            var pagingRequest = GetSinglePagingRequest();
            var users = Core.GetUsers(pagingRequest, groupId);

            var usersViewModel = new UsersViewModel
            {
                Users = users.Items.Select(Mapper.Map<User, UserViewModel>).ToList(),
                Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, users.PagingStatus, pagingRequest)
            };

            return View(usersViewModel);
        }

        [HttpGet, RoleAuthorize(UserRole.Administrator), ImportViewData, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.AddUser)]
        public ActionResult Add()
        {
            if (ViewData.Model != null)
                return View(ViewData.Model);

            var users = Core.GetUsers();
            if (users.PagingStatus.TotalItems >= Core.AppContext.CurrentUser.Organisation.PaymentPlan.MaximumUsers)
            {
                SetNotification(AddUserStatus.PlanThresholdReached, Resources.Account.ResourceManager);
                return RedirectToAction("upgrade", "admin");
            }

            var groups = Core.GetGroups();

            var viewModel = new AddUserViewModel
            {
                Groups = groups.Items.Select(g => Mapper.Map(g, new GroupViewModel())).ToList(),
            };

            return View(viewModel);
        }

		[HttpPost, RoleAuthorize(UserRole.Administrator), ExportViewData]
		public ActionResult ResendInvite(string userId)
		{
			var user = Core.Session.Raven.Load<User>(Errordite.Core.Domain.Organisation.User.GetId(userId));

			if(user != null)
			{
				_sendNotificationCommand.Invoke(new SendNotificationRequest
				{
					EmailInfo = new NewUserEmailInfo
					{
						To = user.Email,
						Token = _encryptor.Encrypt("{0}|{1}".FormatWith(user.PasswordToken.ToString(), Core.AppContext.CurrentUser.Organisation.FriendlyId)).Base64Encode(),
						UserName = user.FirstName
					},
					OrganisationId = Core.AppContext.CurrentUser.OrganisationId
				});

				ConfirmationNotification("A new invite has been sent to {0}".FormatWith(user.Email));
			}
			else
			{
				ErrorNotification("Failed to locate the user with Id {0}".FormatWith(userId));	
			}

			return RedirectToAction("index");
		}

        [HttpPost, RoleAuthorize(UserRole.Administrator), ExportViewData]
        public ActionResult Add(AddUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "add");
            }

            var result = _addUserCommand.Invoke(new AddUserRequest
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Administrator = viewModel.IsAdministrator,
                Organisation = Core.AppContext.CurrentUser.Organisation,
                Email = viewModel.Email,
                GroupIds = viewModel.Groups.Where(g => g.Selected).Select(g => g.Id.GetFriendlyId()).ToList(),
                TimezoneId = viewModel.TimezoneId.IsNullOrEmpty() ? null : viewModel.TimezoneId,
            });

            if (result.Status != AddUserStatus.Ok)
            {
                return RedirectWithViewModel(viewModel, "add", result.Status.MapToResource(Resources.Account.ResourceManager));
            }

            return Redirect(Url.Users());
        }

        [HttpPost, ExportViewData]
        public ActionResult Edit(EditUserViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                if (viewModel.CurrentUser)
                    return RedirectWithViewModel(viewModel, "edit");

                return RedirectWithViewModel(viewModel, "edituser", routeValues: new { userId = viewModel .UserId});
            }

            var result = _editUserCommand.Invoke(new EditUserRequest
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                UserId = viewModel.UserId,
                CurrentUser = Core.AppContext.CurrentUser,
                GroupIds = viewModel.CurrentUser ? null : viewModel.Groups.Where(g => g.Selected).Select(g => g.Id.GetFriendlyId()).ToList(),
                Administrator = (!AppContext.CurrentUser.IsAdministrator() || viewModel.CurrentUser) ? null : (bool?)viewModel.IsAdministrator,
                TimezoneId = viewModel.TimezoneId.IsNullOrEmpty() ? null : viewModel.TimezoneId,
            });

            if (viewModel.CurrentUser)
                return RedirectWithViewModel(viewModel, "yourdetails", result.Status.MapToResource(Resources.Account.ResourceManager), result.Status != EditUserStatus.Ok);
            
            return RedirectWithViewModel(viewModel, "index", result.Status.MapToResource(Resources.Account.ResourceManager), result.Status != EditUserStatus.Ok, new {userId = viewModel.UserId});
        }

		[HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.EditYourDetails)]
        public ActionResult YourDetails()
        {
            return EditUser(Core.AppContext.CurrentUser, true);
        }

        [HttpGet, ImportViewData, RoleAuthorize(UserRole.Administrator), GenerateBreadcrumbs(BreadcrumbId.EditUser)]
        public ActionResult Edit(string userId)
        {
            return EditUser(_getUserQuery.Invoke(new GetUserRequest
            {
                OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                UserId = userId
            }).User, false);
        }

        private ActionResult EditUser(User userProfile, bool currentUser)
        {
            if (ViewData.Model != null)
                return View("edit", ViewData.Model);

            var groups = Core.GetGroups();
            
            var viewModel = new EditUserViewModel
            {
                Email = userProfile.Email,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                UserId = userProfile.Id,
                Groups = groups.Items.Select(g => Mapper.Map(g, new GroupViewModel
                {
                    Selected = userProfile.GroupIds != null && userProfile.GroupIds.Any(ug => ug == g.Id),
                    Disabled = currentUser && !userProfile.IsAdministrator()
                })).ToList(),
                CurrentUser = currentUser,
                TimezoneId = userProfile.TimezoneId,
                IsAdministrator = userProfile.IsAdministrator()
            };

            return View("edit", viewModel);
        }

        [HttpPost, ExportViewData, RoleAuthorize(UserRole.Administrator)]
        public ActionResult Delete(string userId)
        {
            var response = _deleteUserCommand.Invoke(new DeleteUserRequest
            {
                UserId = userId,
                CurrentUser = Core.AppContext.CurrentUser
            });

            if (response.Status != DeleteUserStatus.Ok)
                ErrorNotification(response.Status.MapToResource(Resources.Account.ResourceManager));
            else
                ConfirmationNotification(Resources.Account.DeleteUserStatus_Ok);

            return Redirect(Url.Users());
        }
    }
}
