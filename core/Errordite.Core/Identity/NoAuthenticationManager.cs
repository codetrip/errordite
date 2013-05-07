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

        public AuthenticationIdentity SignInGuest()
        {
            throw new System.NotImplementedException();
        }

        public AuthenticationIdentity GetCurrentUser()
        {
            throw new System.NotImplementedException();
        }
    }
}