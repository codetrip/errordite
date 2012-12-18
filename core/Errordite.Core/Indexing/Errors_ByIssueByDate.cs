using System;
using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Errors_ByIssueByDate : AbstractMultiMapIndexCreationTask<ByDateReduceResult>
    {
        public Errors_ByIssueByDate()
        {
			AddMap<IssueDailyCount>(docs => 
				from dailyCount in docs
				select new
				{
					IssueId = dailyCount.IssueId,
					Date = dailyCount.Date.Date,
					Count = dailyCount.Count,
				});

            Reduce = results => from result in results
                                group result by new { result.IssueId, result.Date }
                                    into hour
                                    select new
                                    {
                                        hour.Key.IssueId,
                                        hour.Key.Date,
                                        Count = hour.Sum(m => m.Count)
                                    };
        }
    }

    public class ByDateReduceResult
    {
        public DateTime Date { get; set; }
        public string IssueId { get; set; }
        public int Count { get; set; }
    }
}