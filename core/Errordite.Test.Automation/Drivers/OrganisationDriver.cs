
using CodeTrip.Core.Session;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Indexing;

namespace Errordite.Test.Automation.Drivers
{
    public interface IOrganisationDriver
    {
        User GetUser(string email);
    }

    public class OrganisationDriver : SessionAccessBase, IOrganisationDriver
    {
        public User GetUser(string email)
        {
            return Session.Raven.Query<User, Users_Search>()
                .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                .FirstOrDefault(u => u.Email == email);
        }
    }
}   

