using System.Web;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Identity
{
    public class AppContext
    {
        /// <summary>
        /// The idea in this is that you get a null AppContext if some error prevents the creation of the proper one.
        /// </summary>
        public static AppContext Null
        {
            get 
            {
                return new AppContext
                {
                    Authentication = new NoAuthenticationManager(),
                    AuthenticationStatus = AuthenticationStatus.Anonymous,
                    CurrentUser = new User()
                };
            }
        }

        private const string AppContextKey = "app_context";

        internal AppContext()
        { }

        public User CurrentUser { get; set; }
        public AuthenticationStatus AuthenticationStatus { get; set; }
        public IAuthenticationManager Authentication { get; set; }
        public AppContext ImpersonatorAppContext { get; set; }

        public static void AddToHttpContext(AppContext context)
        {
            HttpContext.Current.Items[AppContextKey] = context;
        }

        public static AppContext GetFromHttpContext()
        {
            return HttpContext.Current.Items[AppContextKey] as AppContext;
        }

        public static void RemoveFromHttpContext()
        {
            if (HttpContext.Current.Items.Contains(AppContextKey))
                HttpContext.Current.Items.Remove(AppContextKey);
        }
    }

    public enum AuthenticationStatus
    {
        Anonymous,
        NotAuthenticated,
        Authenticated
    }
}