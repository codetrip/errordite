using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Domain.Error;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class IssueDocument
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }
        public string Status { get; set; }
        public int ErrorCount { get; set; }
        public string RulesHash { get; set; }
        public string Id { get; set; }
        public int FriendlyId { get; set; }
        public int MatchPriority { get; set; }
        public DateTime LastErrorUtc { get; set; }
    }

    public class Issues_Search : AbstractIndexCreationTask<Issue, IssueDocument>
    {
        public Issues_Search()
        {
            Map = issues => from doc in issues
                            select new
                            {
                                doc.ApplicationId,
                                doc.LastErrorUtc,
                                doc.Status,
                                doc.UserId,
                                doc.Id,
                                doc.Name,
                                doc.OrganisationId,
                                doc.ErrorCount,
                                doc.RulesHash,
                                FriendlyId = int.Parse(doc.Id.Split('/')[1])
                            };

            Indexes = new Dictionary<Expression<Func<IssueDocument, object>>, FieldIndexing>
            {
                {e => e.ApplicationId, FieldIndexing.Analyzed},
                {e => e.LastErrorUtc, FieldIndexing.Analyzed},
                {e => e.Status, FieldIndexing.Analyzed},
                {e => e.UserId, FieldIndexing.Analyzed},
                {e => e.Id, FieldIndexing.Analyzed},
                {e => e.Name, FieldIndexing.Analyzed},
                {e => e.OrganisationId, FieldIndexing.Analyzed},
                {e => e.ErrorCount, FieldIndexing.Analyzed},
                {e => e.RulesHash, FieldIndexing.Analyzed},
                {e => e.FriendlyId, FieldIndexing.Analyzed}
            };

            Stores = new Dictionary<Expression<Func<IssueDocument, object>>, FieldStorage>
            {
                {e => e.ApplicationId, FieldStorage.No},
                {e => e.LastErrorUtc, FieldStorage.No},
                {e => e.Status, FieldStorage.No},
                {e => e.UserId, FieldStorage.No},
                {e => e.Id, FieldStorage.No},
                {e => e.Name, FieldStorage.No},
                {e => e.OrganisationId, FieldStorage.No},
                {e => e.ErrorCount, FieldStorage.No},
                {e => e.RulesHash, FieldStorage.No},
                {e => e.FriendlyId, FieldStorage.No}
            };

            Sort(e => e.LastErrorUtc, SortOptions.String);
            Sort(e => e.ErrorCount, SortOptions.Int);
            Sort(e => e.FriendlyId, SortOptions.Int);

            Analyzers = new Dictionary<Expression<Func<IssueDocument, object>>, string>
            {
                { e => e.ApplicationId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Status, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.UserId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Id, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.OrganisationId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.ErrorCount, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.RulesHash, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.FriendlyId, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };
        }
    }
}