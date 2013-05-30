using System.Collections.Specialized;
using System.Web;
using Errordite.Core.Extensions;

namespace Errordite.Core.Identity
{
    public class CookieIdentity
    {
        public string Email { get; set; }
        public bool RememberMe { get; set; }
        public bool HasUserProfile { get; set; }

        public string Encode()
        {
            return "{0}={1}&{2}={3}&{4}={5}".FormatWith(
                CoreConstants.Authentication.RememberMe,
                RememberMe.ToString().ToLowerInvariant(),
                CoreConstants.Authentication.Email,
                HttpUtility.UrlEncode(Email),
                CoreConstants.Authentication.HasUserProfile,
                HasUserProfile.ToString().ToLowerInvariant());
        }

        public static CookieIdentity Decode(string cookieValue)
        {
            ArgumentValidation.NotEmpty(cookieValue, "cookieValue");

            NameValueCollection cookieValues = HttpUtility.ParseQueryString(cookieValue);

            if (cookieValues.Get(CoreConstants.Authentication.HasUserProfile) != null &&
                cookieValues.Get(CoreConstants.Authentication.RememberMe) != null &&
                cookieValues.Get(CoreConstants.Authentication.Email) != null)
            {
                var identity = new CookieIdentity
                {
                    HasUserProfile = cookieValues[CoreConstants.Authentication.HasUserProfile] == "true",
                    RememberMe = cookieValues[CoreConstants.Authentication.RememberMe] == "true",
                    Email = cookieValues[CoreConstants.Authentication.Email]
                };  

                return identity;
            }

            return null;
        }
    }
}