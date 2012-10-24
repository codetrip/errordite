using System;
using System.Linq;
using System.Text;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Organisation;
using Raven.Abstractions.Linq;
using Raven.Client;
using Raven.Client.Linq;
using CodeTrip.Core.Extensions;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Auditing.Queries
{
    public class GetAuditRecordsQuery : SessionAccessBase, IGetAuditRecordsQuery
    {
        public GetAuditRecordsResponse Invoke(GetAuditRecordsRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;
            IDocumentQuery<AuditRecord> entities = Session.Raven.Advanced.LuceneQuery<AuditRecord>(CoreConstants.IndexNames.Audit);

            var luceneQuery = new StringBuilder(" OrganisationId:{0}".FormatWith(Organisation.GetId(request.OrganisationId)));

            if (request.CompletedStartDate.HasValue && request.CompletedEndDate.HasValue)
            {
                if (request.CompletedEndDate.Value.Hour == 0 && request.CompletedEndDate.Value.Minute == 0 && request.CompletedEndDate.Value.Second == 0)
                    request.CompletedEndDate = request.CompletedEndDate.Value.AddDays(1).AddSeconds(-1);

                luceneQuery.Append(" AND CompletedOnUtc:[{0} TO {1}]".FormatWith(
                    DateTools.DateToString(request.CompletedStartDate.Value, DateTools.Resolution.MILLISECOND),
                    DateTools.DateToString(request.CompletedEndDate.Value, DateTools.Resolution.MILLISECOND)));
            }

            if (request.Status.HasValue)
            {
                Trace("...Status:={0}", request.Status);
                luceneQuery.Append(" AND Status:{0}".FormatWith(request.Status.ToString()));
            }

            if (request.AuditRecordType.HasValue)
            {
                Trace("...Type:={0}", request.AuditRecordType);
                luceneQuery.Append(" AND Type:{0}".FormatWith(request.AuditRecordType.ToString()));
            }

            var retrievedEntities = entities
                .Where(luceneQuery.ToString())
                .Skip((request.Paging.PageNumber - 1) * request.Paging.PageSize)
                .Take(request.Paging.PageSize)
                .ConditionalSort(request.Paging.Sort, request.Paging.SortDescending, request.Paging.Sort.IsNotNullOrEmpty())
                .Statistics(out stats)
                .ToList();

            var page = new Page<AuditRecord>(retrievedEntities, new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            Trace("...Located {0} Items from page {1}, page size:={2}", stats.TotalResults, request.Paging.PageNumber, request.Paging.PageSize);

            return new GetAuditRecordsResponse
            {
                AuditRecords = page
            };
        }
    }

    public interface IGetAuditRecordsQuery : IQuery<GetAuditRecordsRequest, GetAuditRecordsResponse>
    { }

    public class GetAuditRecordsResponse
    {
        public Page<AuditRecord> AuditRecords { get; set; }
    }

    public class GetAuditRecordsRequest
    {
        public string OrganisationId { get; set; }
        public DateTime? CompletedStartDate { get; set; }
        public DateTime? CompletedEndDate { get; set; }
        public AuditRecordStatus? Status { get; set; }
        public AuditRecordType? AuditRecordType { get; set; }
        public PageRequestWithSort Paging { get; set; }
    }
}
