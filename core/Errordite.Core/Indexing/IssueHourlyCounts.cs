using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
	public class IssueHourlyCounts : AbstractIndexCreationTask<IssueHourlyCount>
	{
        public IssueHourlyCounts()
		{
			Map = hourlyCounts => 
				from count in hourlyCounts
				select new
				{
					count.IssueId,
                    count.ApplicationId
				};
		}
	}
}