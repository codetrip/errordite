namespace Errordite.Core.Identity
{
    public class NoAuthenticationManager : IAuthenticationManager
    {
        public void SignIn(string id, string name)
        {
            throw new System.NotImplementedException();
        }

        public void SignIn(string id, string organisationId, string name)
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

        public void UpdateIdentity(string emailAddress)
        {
            throw new System.NotImplementedException();
        }
    }
}