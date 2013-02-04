using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Groups_Search : AbstractIndexCreationTask<Group>
    {
        public Groups_Search()
        {
            Map = groups => from g in groups
                            select new
                            {
                                g.Id,
                                g.Name
                            };
        }
    }
}