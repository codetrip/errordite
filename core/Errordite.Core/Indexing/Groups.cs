using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Groups : AbstractIndexCreationTask<Group>
    {
        public Groups()
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