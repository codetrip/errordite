using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.MicroKernel.Registration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Matching;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class IssueDocument
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public string ApplicationId { get; set; }
        public string Status { get; set; }
        public int ErrorCount { get; set; }
        public string RulesHash { get; set; }
        public string Id { get; set; }
        public int FriendlyId { get; set; }
        public int MatchPriority { get; set; }
        public DateTime LastErrorUtc { get; set; }
        public string Query { get; set; }
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
                                doc.ErrorCount,
                                doc.RulesHash,
                                FriendlyId = int.Parse(doc.Id.Split('/')[1]),
                                Query = new[] { doc.Name }.Union(doc.Rules.Select(r => r.SearchString)),   
                            };

            Indexes = new Dictionary<Expression<Func<IssueDocument, object>>, FieldIndexing>
            {
                {e => e.Name, FieldIndexing.Analyzed},
            };

            Analyzers = new Dictionary<Expression<Func<IssueDocument, object>>, string>
                {
                    {e => e.Name, typeof(SimpleAnalyzer).FullName}, //SimpleAnalyzer tokenizes on all non-alphanumeric characters
                    {e => e.Query, typeof(SimpleAnalyzer).FullName}, //SimpleAnalyzer tokenizes on all non-alphanumeric characters
                };
            
            Sort(e => e.LastErrorUtc, SortOptions.String);
            Sort(e => e.ErrorCount, SortOptions.Int);
            Sort(e => e.FriendlyId, SortOptions.Int);
        }
    }
}