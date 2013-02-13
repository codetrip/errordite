using System;
using System.Linq;
using Errordite.Core.Domain.Error;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class HistoryDocument
    {
        public string UserId { get; set; }
        public string IssueId { get; set; }
        public string ApplicationId { get; set; }
        public DateTime DateAddedUtc { get; set; }
    }

    public class History_Search : AbstractIndexCreationTask<IssueHistory, HistoryDocument>
    {
        public History_Search()
        {
            Map = history => from doc in history
                            select new
                            {
                                doc.IssueId,
                                doc.ApplicationId,
                                doc.UserId,
                                doc.DateAddedUtc
                            };

            Sort(e => e.DateAddedUtc, SortOptions.String);
        }
    }
}