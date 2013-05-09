namespace Errordite.Core.Identity
{
    public class NoAuthenticationManager : IAuthenticationManager
    {
        public void SignIn(string email)
        {
            throw new System.NotImplementedException();
        }

        public void SignOut()
        {
            throw new System.NotImplementedException();
        }

        public CookieIdentity SignInGuest()
        {
            throw new System.NotImplementedException();
        }

        public CookieIdentity GetCurrentUser()
        {
            throw new System.NotImplementedException();
        }
    }
}