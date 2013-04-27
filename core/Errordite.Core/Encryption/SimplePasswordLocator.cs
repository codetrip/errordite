
using Errordite.Core.Interfaces;

namespace Errordite.Core.Encryption
{
    /// <summary>
    /// SimplePasswordLocator returns the password passed into the constructor of this class
    /// </summary>
    public class SimplePasswordLocator : IPasswordLocator
    {
        private readonly string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePasswordLocator"/> class.
        /// </summary>
        /// <param name="password">The password.</param>
        public SimplePasswordLocator(string password)
        {
            _password = password;
        }

        public string Locate()
        {
            return _password;
        }
    }
}
