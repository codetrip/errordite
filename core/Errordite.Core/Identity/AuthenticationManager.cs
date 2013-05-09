using System;
using System.Web;
using System.Web.Security;
using Errordite.Core.Web;
using Errordite.Core.Extensions;

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
        /// Method should only be invoked for users who have just successfully logged into the site
        /// </summary>
        /// <param name="email">the email they logged in with (could be username or email address)</param>
        public void SignIn(string email)
        {
            FormsAuthentication.SetAuthCookie(email, true);

            var authenticationIdentity = new CookieIdentity
            {
                RememberMe = true,
                Email = email,
                HasUserProfile = true
            };

			_cookieManager.Set(
				CoreConstants.Authentication.IdentityCookieName, 
				authenticationIdentity.Encode(), 
				authenticationIdentity.RememberMe ? DateTime.MaxValue : (DateTime?)null);
            
            //remove the current user from the http context
            AppContext.RemoveFromHttpContext();
        }

        /// <summary>
        /// Creates an identity cookie for our guest and returns their new identity
        /// </summary>
        /// <returns></returns>
        public CookieIdentity SignInGuest()
        {
            //create the new anonymous identity
            var authenticationIdentity = new CookieIdentity
            {
                RememberMe = false,
                Email = CoreConstants.Authentication.GuestUserName,
                HasUserProfile = false
            };

            //update the identity cookie
            _cookieManager.Set(CoreConstants.Authentication.IdentityCookieName, authenticationIdentity.Encode(), DateTime.MaxValue);

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

        public CookieIdentity GetCurrentUser()
        {
            var name = HttpContext.Current.User.Identity.Name;
	        var authCookie = _cookieManager.Get(CoreConstants.Authentication.IdentityCookieName);

	        if (name.IsNullOrEmpty() || authCookie.IsNullOrEmpty())
		        return SignInGuest();

			var authIdentity = CookieIdentity.Decode(authCookie);

			//make sure the identity cookie has not been hacked
	        if (authIdentity.Email.ToLowerInvariant().Trim() == name.ToLowerInvariant().Trim())
		        return authIdentity;

	        return SignInGuest();
        }
    }
}