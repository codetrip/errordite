using System;
using System.Collections.Generic;
using System.Web;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;
using System.Linq;
using Errordite.Core.Web;
using Errordite.Core.Extensions;

namespace Errordite.Core.Identity
{
    public interface IAppContextFactory
    {
        AppContext Create();
        bool TryChangeOrg(string orgId);
    }

    public class AppContextFactory : ComponentBase, IAppContextFactory, IWantToBeProfiled, IImpersonationManager
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly IImpersonationManager _impersonationManager;
        private readonly IAppSession _session;
        private readonly IGetOrganisationsByEmailAddressCommand _getOrganisationsByEmailAddressCommand;
	    private readonly ICookieManager _cookieManager;
	    private readonly IGetUserByEmailAddressQuery _getUserByEmailAddressQuery;

        public AppContextFactory(IAuthenticationManager authenticationManager, 
            IGetOrganisationsByEmailAddressCommand getOrganisationsByEmailAddressCommand, 
            IAppSession session, 
			ICookieManager cookieManager, 
			IGetUserByEmailAddressQuery getUserByEmailAddressQuery)
        {
            _authenticationManager = authenticationManager;
            _getOrganisationsByEmailAddressCommand = getOrganisationsByEmailAddressCommand;
            _session = session;
	        _cookieManager = cookieManager;
	        _getUserByEmailAddressQuery = getUserByEmailAddressQuery;
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
                        var impersonatedIdentity = new CookieIdentity
                        {
                            HasUserProfile = true,
                            Email = impersonationStatus.EmailAddress,
                        };
                        
                        appContext = CreateKnownUser(impersonatedIdentity);
                        appContext.Impersonated = true;
                    }
                    else
                    {
                        var currentAuthenticationIdentity = _authenticationManager.GetCurrentUser();

                        appContext = currentAuthenticationIdentity.HasUserProfile ?
                            CreateKnownUser(currentAuthenticationIdentity) :
                            CreateAnonymousUser();    
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

        private AppContext CreateKnownUser(CookieIdentity cookieIdentity)
        {
	        List<Organisation> organisations;
			var organisation = GetActiveOrganisation(cookieIdentity.Email, out organisations);

	        if (organisation != null)
            {
                _session.SetOrganisation(organisation);

				var user = _getUserByEmailAddressQuery.Invoke(new GetUserByEmailAddressRequest
				{
					EmailAddress = cookieIdentity.Email,
					OrganisationId = organisation.Id
				}).User;

				if (user != null)
				{
					var appContext = new AppContext();

					user.ActiveOrganisation = organisation;
					user.Organisations = organisations;
					user.OrganisationId = organisation.Id;

					appContext.Authentication = _authenticationManager;
					appContext.CurrentUser = user;
					appContext.AuthenticationStatus = HttpContext.Current.Request.IsAuthenticated
						? AuthenticationStatus.Authenticated
						: AuthenticationStatus.NotAuthenticated;

					return appContext;
				}
            }

            return CreateAnonymousUser();
        }

		private Organisation GetActiveOrganisation(string email, out List<Organisation> organisations)
		{
			organisations = _getOrganisationsByEmailAddressCommand.Invoke(new GetOrganisationsByEmailAddressRequest
			{
				EmailAddress = email,
			}).Organisations.Where(o => o.Status != OrganisationStatus.Suspended).ToList();

			var sessionOrganisationId = _cookieManager.Get(CoreConstants.OrganisationIdCookieKey);

			var organisation = sessionOrganisationId.IsNullOrEmpty() ?
				organisations.FirstOrDefault() :
				organisations.FirstOrDefault(o => o.Id == Organisation.GetId(sessionOrganisationId));

			if (organisation == null)
			{
				_cookieManager.Set(CoreConstants.OrganisationIdCookieKey, string.Empty, DateTime.UtcNow.AddDays(-1));
				return organisations.FirstOrDefault();
			}

			_cookieManager.Set(CoreConstants.OrganisationIdCookieKey, organisation.FriendlyId, DateTime.UtcNow.AddMonths(1));
			return organisation;
		}

        public bool TryChangeOrg(string orgId)
        {
            var context = Create();

            List<Organisation> organisations;
            GetActiveOrganisation(context.CurrentUser.Email, out organisations);

            var newOrg = organisations.FirstOrDefault(o => o.Id == Organisation.GetId(orgId));
            
            if (newOrg == null)
                return false;
            
            _cookieManager.Set(CoreConstants.OrganisationIdCookieKey, newOrg.FriendlyId, DateTime.UtcNow.AddMonths(1));
            _session.SetOrganisation(newOrg, true);
            context.CurrentUser.OrganisationId = newOrg.Id;
            context.CurrentUser.ActiveOrganisation = newOrg;
            return true;

        }

        private AppContext CreateAnonymousUser()
        {
            var appContext = new AppContext
            {
                CurrentUser = new User
                {
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
                    StopImpersonating();
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
            StopImpersonating();
        }

        private void StopImpersonating()
        {
            var impersonationStatus = HttpContext.Current.Session["ImpersonationStatus"] as ImpersonationStatus;

            if (impersonationStatus == null || !impersonationStatus.Impersonating)
                return;

            HttpContext.Current.Session["ImpersonationStatus"] = null;
        }
    }
}