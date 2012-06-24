﻿
using System.Web.Mvc;
using Errordite.Core.Authentication.Commands;
using Errordite.Core.Identity;
using Errordite.Core.Organisations.Commands;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Authentication;
using Errordite.Web.Extensions;
using CodeTrip.Core.Extensions;
using Resources;

namespace Errordite.Web.Controllers
{
    public class AuthenticationController : ErrorditeController
    {
        private readonly IAuthenticateUserCommand _authenticateUserCommand;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ISetPasswordCommand _setPasswordCommand;
        private readonly IResetPasswordCommand _resetPasswordCommand;
        private readonly ICreateOrganisationCommand _createOrganisationCommand;

        public AuthenticationController(IAuthenticateUserCommand authenticateUserCommand, 
            IAuthenticationManager authenticationManager, 
            ISetPasswordCommand setPasswordCommand, 
            IResetPasswordCommand resetPasswordCommand, 
            ICreateOrganisationCommand createOrganisationCommand)
        {
            _authenticateUserCommand = authenticateUserCommand;
            _authenticationManager = authenticationManager;
            _setPasswordCommand = setPasswordCommand;
            _resetPasswordCommand = resetPasswordCommand;
            _createOrganisationCommand = createOrganisationCommand;
        }

        [HttpGet]
        public ActionResult Signout()
        {
            _authenticationManager.SignOut();
            return Redirect(Url.Home());
        }

        [HttpGet]
        public ActionResult Password(string token)
        {
            return View(ViewData.Model == null ? new PasswordViewModel { Token = token } : ViewData.Model as PasswordViewModel);
        }

        [HttpPost]
        public ActionResult Password(PasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "password");
            }

            var result = _setPasswordCommand.Invoke(new SetPasswordRequest
            {
                Password = viewModel.Password,
                Token = viewModel.Token
            });

            if (result.Status != SetPasswordStatus.Ok)
            {
                return RedirectWithViewModel(viewModel, "password", result.Status.MapToResource(Resources.Authentication.ResourceManager));
            }

            return Redirect(Url.SignIn());
        }

        [HttpGet, ImportViewData]
        public ActionResult SignIn()
        {
            return View(ViewData.Model == null ? new LoginViewModel() : ViewData.Model as LoginViewModel);
        }

        [HttpPost, ExportViewData]
        public ActionResult SignIn(LoginViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "signin");
            }

            var result = _authenticateUserCommand.Invoke(new AuthenticateUserRequest
            {
                Email = viewModel.Email,
                Password = viewModel.Password
            });

            if(result.Status != AuthenticateUserStatus.Ok)
            {
                return RedirectWithViewModel(viewModel, "signin", result.Status.MapToResource(Authentication.ResourceManager));
            }

            _authenticationManager.SignIn(result.UserId, result.OrganisationId, viewModel.Email);
            return Redirect(viewModel.ReturnUrl.IsNullOrEmpty() ? Url.Dashboard() : viewModel.ReturnUrl);
        }

        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            return View(ViewData.Model == null ? new ResetPasswordViewModel() : ViewData.Model as ResetPasswordViewModel);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "password");
            }

            var result = _resetPasswordCommand.Invoke(new ResetPasswordRequest
            {
                Email = viewModel.Email
            });

            if (result.Status != ResetPasswordStatus.Ok)
            {
                return RedirectWithViewModel(viewModel, "resetpassword", result.Status.MapToResource(Authentication.ResourceManager));
            }

            return Redirect(Url.SignIn());
        }

        [HttpPost, ExportViewData]
        public ActionResult SignUp(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "signup");
            }

            var response = _createOrganisationCommand.Invoke(new CreateOrganisationRequest
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                OrganisationName = viewModel.OrganisationName,
                Password = viewModel.Password
            });

            if (response.Status != CreateOrganisationStatus.Ok)
            {
                return RedirectWithViewModel(viewModel, "signup", response.Status.MapToResource(Account.ResourceManager));
            }

            _authenticationManager.SignIn(response.UserId, response.OrganisationId, viewModel.Email);
            return Redirect(Url.Dashboard());
        }

        [HttpGet, ImportViewData]
        public ActionResult SignUp()
        {
            return View(ViewData.Model == null ? new RegisterViewModel() : ViewData.Model as RegisterViewModel);
        }
    }
}
