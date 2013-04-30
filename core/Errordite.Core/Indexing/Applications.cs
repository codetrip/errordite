using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Applications : AbstractIndexCreationTask<Application>
    {
        public Applications()
        {
            Map = applications => 
                from a in applications
                select new
                {
                    a.Id,
                    a.Name,
                    a.Token,
                    a.DefaultUserId
                };
        }
    }
}