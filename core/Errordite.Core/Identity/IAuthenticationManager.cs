using Errordite.Core.Interfaces;

namespace Errordite.Core.Identity
{
    public interface IAuthenticationManager : IWantToBeProfiled
    {
        void SignIn(string id, string organisationId, string name);
        void SignOut();
        AuthenticationIdentity SignInGuest();
        AuthenticationIdentity GetCurrentUser();
        void UpdateIdentity(string emailAddress);
    }
}
