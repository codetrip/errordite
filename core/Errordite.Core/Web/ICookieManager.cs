using System;

namespace Errordite.Core.Web
{
    public interface ICookieManager
    {
        string Get(string cookieName);
        string Get(string cookieName, string cookieKey);

        void Set(string cookieName, string cookieValue, DateTime? expires);
        void Set(string cookieName, string cookieKey, string cookieValue, DateTime? expires);

        void Expire(string cookieName);
    }
}