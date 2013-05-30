using Errordite.Core.Interfaces;

namespace Errordite.Core.Identity
{
    public interface IAuthenticationManager : IWantToBeProfiled
    {
        void SignIn(string email);
        void SignOut();
        CookieIdentity SignInGuest();
        CookieIdentity GetCurrentUser();
    }
}
