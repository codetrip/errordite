using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
	public class IssueDailyCounts : AbstractIndexCreationTask<IssueDailyCount>
	{
		public IssueDailyCounts()
		{
			Map = dailyCounts => 
				from count in dailyCounts
				select new
				{
					count.IssueId,
                    count.Date,
                    count.Count,
                    count.ApplicationId,
                    count.Historical
				};

            Sort(e => e.Date, SortOptions.String);
		}
	}
}