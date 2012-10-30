using System.Linq;
using Errordite.Core.Domain.Organisation;
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