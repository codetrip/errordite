using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Organisations_Search : AbstractIndexCreationTask<Organisation>
    {
        public Organisations_Search()
        {
            Map = organisations => from o in organisations
                            select new
                            {
                                o.Id,
                                o.Name
                            };
        }
    }
}