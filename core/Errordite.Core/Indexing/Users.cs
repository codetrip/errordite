using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Users : AbstractIndexCreationTask<User>
    {
        public Users()
        {
            Map = users => from user in users
                            select new
                            {
                                user.Id,
                                user.GroupIds,
                                user.Password,
                                user.PasswordToken,
                                user.Email,
                                user.LastName
                            };
        }
    }
}