using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class IssueExtraDataKeys
    {
        public string IssueId { get; set; }
        public List<string> Keys { get; set; }
    }

    public class IssuesExtraDataKeys : AbstractIndexCreationTask<Error, IssueExtraDataKeys>
    {
        public IssuesExtraDataKeys()
        {
            Map = errors => from doc in errors
                            select new
                                {
                                    doc.IssueId,
                                    Keys =
                                        doc.ContextData.Select(d => d.Key)
                                           .Union(
                                               doc.ExceptionInfos.SelectMany(i => i.ExtraData.Select(d => d.Key))
                                                  .Union(doc.ContextData.Select(c => c.Key))
                                                  .Distinct()),
                                };

            Reduce = keysPerIssue => from k in keysPerIssue
                                     group k by k.IssueId
                                     into issueKeys
                                     select new
                                         {
                                             IssueId = issueKeys.Key,
                                             Keys = issueKeys.SelectMany(p => p.Keys).Distinct().ToList(),
                                         };
        }
    }
}