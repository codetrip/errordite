using System;
using System.Web;
using Errordite.Core;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;

namespace Errordite.Core.Identity
{
    public interface IAppContextFactory
    {
        AppContext Create();
    }

    public class AppContextFactory : ComponentBase, IAppContextFactory, IWantToBeProfiled, IImpersonationManager
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly IGetUserQuery _getUserQuery;
        private readonly IImpersonationManager _impersonationManager;
        private readonly IAppSession _session;
        private readonly IGetOrganisationByEmailAddressCommand _getOrganisationByEmailAddressCommand;

        public AppContextFactory(IAuthenticationManager authenticationManager, 
            IGetUserQuery getUserQuery, 
            IGetOrganisationByEmailAddressCommand getOrganisationByEmailAddressCommand, 
            IAppSession session)
        {
            _authenticationManager = authenticationManager;
            _getUserQuery = getUserQuery;
            _getOrganisationByEmailAddressCommand = getOrganisationByEmailAddressCommand;
            _session = session;
            _impersonationManager = this;
        }

        public AppContext Create()
        {
            try
            {
                AppContext appContext = AppContext.GetFromHttpContext();

                if (appContext == null)
                {
                    var impersonationStatus = _impersonationManager.CurrentStatus;

                    //GT: ideally we'd like to have a record of *who* is impersonating, but this would
                    //    mean loading the impersonating user, which would mean we'd need to set the 
                    //    org on the session twice, which kind of breaks the security model.
                    //    If we really wanted this information, we could set it into the ImpersonationStatus
                    //    when starting the impersonation
                    if (impersonationStatus.Impersonating)
                    {
                        var impersonatedIdentity = new AuthenticationIdentity
                        {
                            HasUserProfile = true,
                            UserId = impersonationStatus.UserId,
                            OrganisationId = impersonationStatus.OrganisationId,
                            Email = impersonationStatus.EmailAddress,
                        };
                        
                        appContext = CreateKnownUser(impersonatedIdentity);
                        appContext.Impersonated = true;
                    }
                    else
                    {
                        AuthenticationIdentity currentAuthenticationIdentity = _authenticationManager.GetCurrentUser();

                        appContext = currentAuthenticationIdentity.HasUserProfile ?
                            CreateKnownUser(currentAuthenticationIdentity) :
                            CreateAnonymousUser(currentAuthenticationIdentity);    
                    }

                    AppContext.AddToHttpContext(appContext);
                }

                return appContext;
            }
            catch (Exception ex)
            {
                //exceptions thrown here cause an exception in the WindsorControllerFactory when the controller is released
                //hence the true exception is masked in the event log.  Hence we explicitly log the exception in here before throwing it
                //TODO: get to the bottom of the actual problem!
                Error(ex);
                throw;
            }
        }

        private AppContext CreateKnownUser(AuthenticationIdentity authenticationIdentity)
        {
            var organisation = _getOrganisationByEmailAddressCommand.Invoke(new GetOrganisationByEmailAddressRequest
                {
                    EmailAddress = authenticationIdentity.Email,
                }).Organisation;

            User user = null;
            if (organisation != null)
            {
                _session.SetOrganisation(organisation);
                user = _getUserQuery.Invoke(new GetUserRequest
                {
                    OrganisationId = organisation.Id, 
                    UserId = User.GetId(authenticationIdentity.UserId)
                }).User;
                user.Organisation = organisation;
            }

            var appContext = new AppContext();

            if (user == null || user.Organisation == null || user.Organisation.Status == OrganisationStatus.Suspended)
            {
                _authenticationManager.SignInGuest();
                return CreateAnonymousUser(_authenticationManager.GetCurrentUser());
            }

            appContext.Authentication = _authenticationManager;
            appContext.CurrentUser = user;
            appContext.AuthenticationStatus = HttpContext.Current.Request.IsAuthenticated
                ? AuthenticationStatus.Authenticated
                : AuthenticationStatus.NotAuthenticated;

            return appContext;
        }

        private AppContext CreateAnonymousUser(AuthenticationIdentity authenticationIdentity)
        {
            var appContext = new AppContext
            {
                CurrentUser = new User
                {
                    Id = authenticationIdentity.UserId,
                    Role = UserRole.Guest
                },
                AuthenticationStatus = AuthenticationStatus.Anonymous,
                Authentication = _authenticationManager,
            };

            return appContext;
        }

        ImpersonationStatus IImpersonationManager.CurrentStatus
        {
            get
            {
                var status = HttpContext.Current.Session["ImpersonationStatus"] as ImpersonationStatus;
                if (status != null && status.ExpiryUtc < DateTime.UtcNow)
                {
                    StopImpersonating(true);
                    status = null;
                }
                return status ?? new ImpersonationStatus { Impersonating = false };
            }
        }

        void IImpersonationManager.Impersonate(ImpersonationStatus impersonationStatus)
        {
            impersonationStatus.ExpiryUtc = DateTime.UtcNow.AddMinutes(30);
            HttpContext.Current.Session["ImpersonationStatus"] = impersonationStatus;
        }

        void IImpersonationManager.StopImpersonating()
        {
            StopImpersonating(false);
        }

        private void StopImpersonating(bool expired)
        {
            var impersonationStatus = HttpContext.Current.Session["ImpersonationStatus"] as ImpersonationStatus;

            if (impersonationStatus == null || !impersonationStatus.Impersonating)
                return;

            HttpContext.Current.Session["ImpersonationStatus"] = null;
        }
    }
}