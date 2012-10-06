
using CodeTrip.Core.Interfaces;

namespace CodeTrip.Core.Encryption
{
    public class PresetPasswordLocator : IPasswordLocator
    {
        public string Locate()
        {
            return "ThisIsTh3Pa55w0rd!";
        }
    }
}
