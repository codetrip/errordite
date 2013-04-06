using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
	public class OrganisationDailyCount_Search : AbstractIndexCreationTask<IssueDailyCount>
	{
		public OrganisationDailyCount_Search()
		{
            Map = docs => 
               from dailyCount in docs
               select new
               {
                   dailyCount.ApplicationId,
                   dailyCount.Date.Date,
                   dailyCount.Count,
               };

            Reduce = results => from result in results
                                group result by new
                                {
                                    result.Date,
                                    result.ApplicationId
                                } 
                                into dailyCount
                                select new
                                {
                                    dailyCount.Key.Date,
                                    dailyCount.Key.ApplicationId,
                                    Count = dailyCount.Sum(m => m.Count)
                                };

            Sort(e => e.Date, SortOptions.String);
		}
	}
}