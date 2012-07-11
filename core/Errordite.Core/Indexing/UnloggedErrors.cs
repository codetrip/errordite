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
    public class UnloggedErrors : AbstractIndexCreationTask<UnloggedError>
    {
        public UnloggedErrors()
        {
            Map = errors => from doc in errors
                            select new
                                {
                                    doc.TimestampUtc,
                                    doc.IssueId,
                                    doc.ApplicationId
                                };

            Indexes = new Dictionary<Expression<Func<UnloggedError, object>>, FieldIndexing>
            {
                {e => e.TimestampUtc, FieldIndexing.Analyzed},
                {e => e.IssueId, FieldIndexing.Analyzed},
                {e => e.ApplicationId, FieldIndexing.Analyzed}
            };

            Stores = new Dictionary<Expression<Func<UnloggedError, object>>, FieldStorage>
            {
                {e => e.TimestampUtc, FieldStorage.No},
                {e => e.IssueId, FieldStorage.No},
                {e => e.ApplicationId, FieldStorage.No},
            };

            Analyzers = new Dictionary<Expression<Func<UnloggedError, object>>, string>
            {
                { e => e.IssueId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.ApplicationId, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };
        }
    }
}