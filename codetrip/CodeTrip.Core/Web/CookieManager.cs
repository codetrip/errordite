using System;
using System.Web;
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Web
{
    /// <summary>
    /// Simple abstraction over Http cookies
    /// </summary>
    public class CookieManager : ICookieManager
    {
        public string Get(string cookieName)
        {
            return Get(cookieName, null);
        }

        public string Get(string cookieName, string cookieKey)
        {
            ArgumentValidation.NotEmpty(cookieName, "cookieName");

            //request cookie should take priority when reading values
            HttpCookie cookie = GetCookie(cookieName);

            if (cookie == null)
                return string.Empty;

            if (cookieKey.IsNullOrEmpty())
                return cookie.Value;

            return cookie.Values[cookieKey];
        }

        public void Set(string cookieName, string cookieValue, DateTime? expires)
        {
            Set(cookieName, null, cookieValue, expires);
        }

        public void Set(string cookieName, string cookieKey, string cookieValue, DateTime? expires)
        {
            ArgumentValidation.NotEmpty(cookieName, "cookieName");

            //response cookie should take priority when writing values
            HttpCookie cookie = GetCookie(cookieName) ?? new HttpCookie(cookieName);

            //all our cookies should not be accessible from client scripts, so mark them as http only
            cookie.HttpOnly = true;

            if (cookieKey.IsNullOrEmpty())
                cookie.Value = cookieValue;
            else
                cookie.Values[cookieKey] = cookieValue;

            //set the expiry if specified, if not specified the cookie will be a session cookie and
            //will be deleted by the browser at the end of the users browser session
            if (expires.HasValue)
                cookie.Expires = expires.Value;

            //remove the current cookie from the request if it exists
            HttpContext.Current.Response.Cookies.Remove(cookieName);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public void Expire(string cookieName)
        {
            HttpCookie expiredCookie = new HttpCookie(cookieName)
            {
                Expires = DateTime.Now.AddDays(-1)
            };

            HttpContext.Current.Response.Cookies.Remove(cookieName);
            HttpContext.Current.Response.Cookies.Add(expiredCookie);
        }

        /// <summary>
        /// Gets the cookie from the response if it is there and has a value (in this
        /// instance we would have already written to the response cookie so we dont
        /// want to overwrite that) otherwise look on the request for the cookie, if
        /// the cookie exists on the request return it, so its value is copied to the
        /// new response cookie.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        private static HttpCookie GetCookie(string cookieName)
        {
            var responseCookie = HttpContext.Current.Response.Cookies[cookieName];

            if (responseCookie == null || responseCookie.Value.IsNullOrEmpty())
            {
                //make sure we remove the cookie which was just created by checking the response for our cookie.
                HttpContext.Current.Response.Cookies.Remove(cookieName);

                var requestCookie = HttpContext.Current.Request.Cookies[cookieName];

                if (requestCookie == null || requestCookie.Value.IsNullOrEmpty())
                {
                    return null;
                }

                return requestCookie;
            }

            return responseCookie;
        }
    }
}