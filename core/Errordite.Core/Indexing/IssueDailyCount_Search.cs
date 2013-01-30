using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
	public class IssueDailyCount_Search : AbstractIndexCreationTask<IssueDailyCount>
	{
		public IssueDailyCount_Search()
		{
			Map = dailyCounts => 
				from count in dailyCounts
				select new
				{
					count.IssueId,
                    count.Date,
                    count.Count,
                    count.CreatedOnUtc,
                    count.OrganisationId,
                    count.ApplicationId
				};

            Sort(e => e.Date, SortOptions.String);
		}
	}
}