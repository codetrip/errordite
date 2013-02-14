using System;
using System.Web;
using System.Web.Security;
using CodeTrip.Core.Web;
using CodeTrip.Core.Extensions;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;

namespace Errordite.Core.Identity
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly ICookieManager _cookieManager;
        private readonly IGetOrganisationByEmailAddressCommand _getOrganisationByEmailAddressCommand;
        private readonly IGetUserByEmailAddressQuery _getUserByEmailAddressQuery;
        private readonly IAppSession _session;

        public AuthenticationManager(ICookieManager cookieManager, 
            IGetOrganisationByEmailAddressCommand getOrganisationByEmailAddressCommand, 
            IGetUserByEmailAddressQuery getUserQuery, 
            IAppSession session)
        {
            _cookieManager = cookieManager;
            _getOrganisationByEmailAddressCommand = getOrganisationByEmailAddressCommand;
            _getUserByEmailAddressQuery = getUserQuery;
            _session = session;
        }

        /// <summary>
        /// If the user changes his email address we need to update the identity cookie or we won't know
        /// who he is on the next request.
        /// </summary>
        public void UpdateIdentity(string emailAddress)
        {
            var authenticationIdentity = GetCurrentUser();

            authenticationIdentity.Email = emailAddress;

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

            var organisation = _getOrganisationByEmailAddressCommand.Invoke(new GetOrganisationByEmailAddressRequest
                {
                    EmailAddress = name,
                }).Organisation;

            if (organisation == null)
                return SignInGuest();

            _session.SetOrganisation(organisation);

            var user = _getUserByEmailAddressQuery.Invoke(new GetUserByEmailAddressRequest
            {
                EmailAddress = name,
            }).User;

            if (user == null)
                return SignInGuest();

            var currentUser = new AuthenticationIdentity
                {
                    Email = user.Email,
                    HasUserProfile = true,
                    OrganisationId = user.OrganisationId,
                    RememberMe = true,
                    UserId = user.Id, //TODO get rid of all the pointless stuff here
                };

            return currentUser;
        }
    }
}