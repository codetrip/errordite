using System.Collections.Specialized;
using System.Web;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Identity
{
    public class AuthenticationIdentity
    {
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        public string Email { get; set; }
        public bool RememberMe { get; set; }
        public bool HasUserProfile { get; set; }

        public string CookieEncode()
        {
            return "{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}".FormatWith(
                CoreConstants.Authentication.RememberMe,
                RememberMe.ToString().ToLowerInvariant(),
                CoreConstants.Authentication.Email,
                Email,
                CoreConstants.Authentication.UserId,
                UserId,
                CoreConstants.Authentication.HasUserProfile,
                HasUserProfile.ToString().ToLowerInvariant(),
                CoreConstants.Authentication.OrganisationId,
                OrganisationId);
        }

        public static AuthenticationIdentity CookieDecode(string cookieValue)
        {
            ArgumentValidation.NotEmpty(cookieValue, "cookieValue");

            NameValueCollection cookieValues = HttpUtility.ParseQueryString(cookieValue);

            if (cookieValues.Get(CoreConstants.Authentication.HasUserProfile) != null &&
                cookieValues.Get(CoreConstants.Authentication.RememberMe) != null &&
                cookieValues.Get(CoreConstants.Authentication.UserId) != null &&
                cookieValues.Get(CoreConstants.Authentication.Email) != null &&
                cookieValues.Get(CoreConstants.Authentication.OrganisationId) != null)
            {
                AuthenticationIdentity identity = new AuthenticationIdentity
                {
                    HasUserProfile = cookieValues[CoreConstants.Authentication.HasUserProfile] == "true",
                    RememberMe = cookieValues[CoreConstants.Authentication.RememberMe] == "true",
                    UserId = cookieValues[CoreConstants.Authentication.UserId],
                    OrganisationId = cookieValues[CoreConstants.Authentication.OrganisationId],
                    Email = cookieValues[CoreConstants.Authentication.Email]
                };  

                return identity;
            }

            return null;
        }
    }
}