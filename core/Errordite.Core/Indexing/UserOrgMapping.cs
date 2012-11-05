using System.Linq;
using Errordite.Core.Domain.Central;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class UserOrgMappings : AbstractIndexCreationTask<UserOrganisationMapping>
    {
         public UserOrgMappings()
         {
             Map = userOrgMappings => from u in userOrgMappings
                                      select new {u.EmailAddress, u.OrganisationId};

         }
    }
}