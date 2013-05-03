using System;
using System.Web;
using System.Web.Security;
using Errordite.Core.Web;
using Errordite.Core.Extensions;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;
using System.Linq;

namespace Errordite.Core.Identity
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly ICookieManager _cookieManager;
        private readonly IGetOrganisationsByEmailAddressCommand _getOrganisationsByEmailAddressCommand;
        private readonly IGetUserByEmailAddressQuery _getUserByEmailAddressQuery;
        private readonly IAppSession _session;

        public AuthenticationManager(ICookieManager cookieManager, 
            IGetOrganisationsByEmailAddressCommand getOrganisationsByEmailAddressCommand, 
            IGetUserByEmailAddressQuery getUserQuery, 
            IAppSession session)
        {
            _cookieManager = cookieManager;
            _getOrganisationsByEmailAddressCommand = getOrganisationsByEmailAddressCommand;
            _getUserByEmailAddressQuery = getUserQuery;
            _session = session;
        }

        /// <summary>
        /// If the user changes his email address we need to update the identity cookie or we won't know
        /// who he is on the next request.
        /// </summary>
        public void UpdateIdentity(string emailAddress, string organisationId, string userId)
        {
            var authenticationIdentity = GetCurrentUser();

            authenticationIdentity.Email = emailAddress;
	        authenticationIdentity.OrganisationId = organisationId;
	        authenticationIdentity.UserId = userId;

            SetIdentityCookie(authenticationIdentity);
        }

        private void SetIdentityCookie(AuthenticationIdentity authenticationIdentity)
        {
            //set the id cookie with no expiry and the values from the AuthenticationIdentity instance
            _cookieManager.Set(CoreConstants.Authentication.IdentityCookieName, authenticationIdentity.CookieEncode(), authenticationIdentity.RememberMe ? DateTime.MaxValue : (DateTime?)null);
        }

        /// <summary>
        /// Method should only be invoked for users who have just successfully logged into the site
        /// </summary>
        /// <param name="id">the users Id</param>
        /// <param name="organisationId"> </param>
        /// <param name="email">the email they logged in with (could be username or email address)</param>
        public void SignIn(string id, string organisationId, string email)
        {
            FormsAuthentication.SetAuthCookie(email, true);

            var authenticationIdentity = new AuthenticationIdentity
            {
                UserId = id.GetFriendlyId(),
                RememberMe = true,
                Email = email,
                HasUserProfile = true,
                OrganisationId = organisationId
            };

            SetIdentityCookie(authenticationIdentity);
            
            //remove the current user from the http context
            AppContext.RemoveFromHttpContext();
        }

        /// <summary>
        /// Creates an identity cookie for our guest and returns their new identity
        /// </summary>
        /// <returns></returns>
        public AuthenticationIdentity SignInGuest()
        {
            //create the new anonymous identity
            var authenticationIdentity = new AuthenticationIdentity
            {
                UserId = Guid.NewGuid().ToString(),
                RememberMe = false,
                Email = CoreConstants.Authentication.GuestUserName,
                HasUserProfile = false
            };

            //update the identity cookie
            _cookieManager.Set(CoreConstants.Authentication.IdentityCookieName, authenticationIdentity.CookieEncode(), DateTime.MaxValue);

            //remove the current user from the http context
            AppContext.RemoveFromHttpContext();

            return authenticationIdentity;
        }

        /// <summary>
        /// Make the user anonymous and sign them out of forms auth
        /// </summary>
        public void SignOut()
        {
            FormsAuthentication.SignOut();
            SignInGuest();
        }

        public AuthenticationIdentity GetCurrentUser()
        {
            var name = HttpContext.Current.User.Identity.Name;

	        if (name.IsNullOrEmpty())
		        return SignInGuest();

            var organisations = _getOrganisationsByEmailAddressCommand.Invoke(new GetOrganisationsByEmailAddressRequest
            {
                EmailAddress = name,
            }).Organisations;

            if (!organisations.Any())
                return SignInGuest();

            var currentUser = new AuthenticationIdentity
            {
				Email = name,
                HasUserProfile = true,
                RememberMe = true,
            };

            return currentUser;
        }
    }
}