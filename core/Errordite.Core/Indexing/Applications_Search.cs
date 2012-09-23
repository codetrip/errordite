using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Applications_Search : AbstractIndexCreationTask<Application>
    {
        public Applications_Search()
        {
            Map = applications => 
                from a in applications
                select new
                {
                    a.Id,
                    a.Name,
                    a.OrganisationId,
                    a.Token,
                    a.DefaultUserId
                };
        }
    }
}