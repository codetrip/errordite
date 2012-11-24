using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Users_Search : AbstractIndexCreationTask<User>
    {
        public Users_Search()
        {
            Map = users => from user in users
                            select new
                            {
                                user.Id,
                                user.GroupIds,
                                user.OrganisationId,
                                user.Password,
                                user.PasswordToken,
                                user.Email,
                                user.LastName
                            };
        }
    }
}