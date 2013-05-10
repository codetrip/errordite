using Errordite.Core.Interfaces;

namespace Errordite.Core.Identity
{
    public interface IAuthenticationManager : IWantToBeProfiled
    {
        void SignIn(string name);
        void SignOut();
        CookieIdentity SignInGuest();
        CookieIdentity GetCurrentUser();
    }
}
