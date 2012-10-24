using System;
using System.Web.Security;
using CodeTrip.Core.Web;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Identity
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly ICookieManager _cookieManager;

        public AuthenticationManager(ICookieManager cookieManager)
        {
            _cookieManager = cookieManager;
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

            AuthenticationIdentity authenticationIdentity = new AuthenticationIdentity
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
            AuthenticationIdentity authenticationIdentity = new AuthenticationIdentity
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
            string currentUser = _cookieManager.Get(CoreConstants.Authentication.IdentityCookieName);

            if(!currentUser.IsNullOrEmpty())
            {
                var authIdentity = AuthenticationIdentity.CookieDecode(currentUser);
                return authIdentity ?? SignInGuest();
            }
            
            return SignInGuest();
        }
    }
}