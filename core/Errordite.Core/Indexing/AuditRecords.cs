using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Domain.Organisation;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class AuditRecords : AbstractIndexCreationTask<AuditRecord>
    {
        public AuditRecords()
        {
            Map = auditrecords => from doc in auditrecords select new
            {
                doc.OrganisationId,
                doc.CompletedOnUtc,
                doc.Status,
                doc.AuditRecordType
            };

            Indexes = new Dictionary<Expression<Func<AuditRecord, object>>, FieldIndexing>
            {
                {e => e.OrganisationId, FieldIndexing.Analyzed},
                {e => e.CompletedOnUtc, FieldIndexing.Analyzed},
                {e => e.Status, FieldIndexing.Analyzed},
                {e => e.AuditRecordType, FieldIndexing.Analyzed}
            };

            Stores = new Dictionary<Expression<Func<AuditRecord, object>>, FieldStorage>
            {
                {e => e.OrganisationId, FieldStorage.No},
                {e => e.CompletedOnUtc, FieldStorage.No},
                {e => e.Status, FieldStorage.No},
                {e => e.AuditRecordType, FieldStorage.No}
            };

            Sort(e => e.CompletedOnUtc, SortOptions.String);

            Analyzers = new Dictionary<Expression<Func<AuditRecord, object>>, string>
            {
                { e => e.OrganisationId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Status, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.AuditRecordType, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };
        }
    }
}