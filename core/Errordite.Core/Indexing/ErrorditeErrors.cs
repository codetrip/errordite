using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Auditing;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class ErrorditeErrors : AbstractIndexCreationTask<ErrorditeError>
    {
        public ErrorditeErrors()
        {
            Map = errors => from doc in errors select new
            {
                doc.Type,
                doc.TimestampUtc,
                doc.Application,
                doc.Text,
                doc.MessageId
            };

            Indexes = new Dictionary<Expression<Func<ErrorditeError, object>>, FieldIndexing>
            {
                {e => e.Type, FieldIndexing.Analyzed},
                {e => e.TimestampUtc, FieldIndexing.Analyzed},
                {e => e.Application, FieldIndexing.Analyzed},
                {e => e.Text, FieldIndexing.Analyzed},
                {e => e.MessageId, FieldIndexing.Analyzed}
            };

            Stores = new Dictionary<Expression<Func<ErrorditeError, object>>, FieldStorage>
            {
                {e => e.Type, FieldStorage.No},
                {e => e.TimestampUtc, FieldStorage.No},
                {e => e.Application, FieldStorage.No},
                {e => e.Text, FieldStorage.No},
                {e => e.MessageId, FieldStorage.No}
            };

            Sort(e => e.TimestampUtc, SortOptions.String);

            Analyzers = new Dictionary<Expression<Func<ErrorditeError, object>>, string>
            {
                { e => e.MessageId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Application, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };
        }
    }
}