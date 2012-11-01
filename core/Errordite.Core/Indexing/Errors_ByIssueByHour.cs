using System;
using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Errors_ByIssueByHour : AbstractMultiMapIndexCreationTask<ByHourReduceResult>
    {
        private void AddErrorMap<T>() where T: ErrorBase
        {
            AddMap<T>(docs => from error in docs
                              select new
                              {
                                  IssueId = error.IssueId,
                                  Hour = new DateTime(1, 1, 1, error.TimestampUtc.Hour, 0, 0),
                                  Count = 1,
                              });
        }

        public Errors_ByIssueByHour()
        {
            AddErrorMap<Error>();
            AddErrorMap<UnloggedError>();

            Reduce = results => from result in results
                                group result by new { result.IssueId, result.Hour }
                                    into hour
                                    select new
                                    {
                                        hour.Key.IssueId,
                                        hour.Key.Hour,
                                        Count = hour.Sum(m => m.Count)
                                    };
        }
    }

    public class ByHourReduceResult
    {
        public DateTime Hour { get; set; }
        public string IssueId { get; set; }
        public int Count { get; set; }
    }
}