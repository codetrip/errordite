using System.Linq;
using Errordite.Core.Domain.Central;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class UserOrganisationMappings : AbstractIndexCreationTask<UserOrganisationMapping>
    {
         public UserOrganisationMappings()
         {
             Map = mappings => from u in mappings
                               select new {u.EmailAddress, u.OrganisationId};
         }
    }
}