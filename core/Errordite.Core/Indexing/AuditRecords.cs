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

            Sort(e => e.CompletedOnUtc, SortOptions.String);

        }
    }
}