using System;
using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class Errors_ByIssueByDate : AbstractMultiMapIndexCreationTask<ByDateReduceResult>
    {
        private void AddErrorMap<T>() where T: ErrorBase
        {
            AddMap<T>(docs => from error in docs
                              select new
                              {
                                  IssueId = error.IssueId,
                                  Date = error.TimestampUtc.Date, //TODO: could have a property on an error called OrganisationLocalTimestamp
                                  Count = 1,
                              });
        }

        public Errors_ByIssueByDate()
        {
            AddErrorMap<Error>();
            AddErrorMap<UnloggedError>();

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