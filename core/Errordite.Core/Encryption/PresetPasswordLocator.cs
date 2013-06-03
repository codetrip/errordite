
using Errordite.Core.Interfaces;

namespace Errordite.Core.Encryption
{
    public class PresetPasswordLocator : IPasswordLocator
    {
        public string Locate()
        {
            return "ThisIsTh3Pa55w0rd!";
        }
    }
}
