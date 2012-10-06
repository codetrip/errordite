using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    /// <summary>
    /// NOTE TO GAZ - in case your wandering why this is still a multimap index - it didnt work as single map index, have not spent the time looking into why not, bit weird though
    /// </summary>
    public class Errors_CountByIssue : AbstractMultiMapIndexCreationTask<ErrorCountByIssueResult>
    {
        public Errors_CountByIssue()
        {
            AddMap<Error>(docs => from error in docs select new { error.IssueId, Count = 1 });
            AddMap<UnloggedError>(docs => from error in docs select new { error.IssueId, Count = 1 });

            Reduce = results => from result in results
                                group result by new { result.IssueId }
                                    into issue
                                    select new
                                    {
                                        issue.Key.IssueId,
                                        Count = issue.Sum(m => m.Count)
                                    };
        }
    }

    public class ErrorCountByIssueResult
    {
        public string IssueId { get; set; }
        public int Count { get; set; }
    }

}