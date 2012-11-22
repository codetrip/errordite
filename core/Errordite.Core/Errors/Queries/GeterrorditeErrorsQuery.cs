using System;
using System.Linq;
using System.Text;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Auditing;
using Raven.Abstractions.Linq;
using Raven.Client;
using Raven.Client.Linq;
using CodeTrip.Core.Extensions;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Errors.Queries
{
    public class GetErrorditeErrorsQuery : SessionAccessBase, IGetErrorditeErrorsQuery
    {
        public GetErrorditeErrorsResponse Invoke(GetErrorditeErrorsRequest request)
        {
            Trace("Starting...");

            RavenQueryStatistics stats;
            IDocumentQuery<ErrorditeError> entities = Session.MasterRaven.Advanced.LuceneQuery<ErrorditeError>(CoreConstants.IndexNames.ErrorditeErrors).Statistics(out stats);

            var luceneQuery = new StringBuilder(string.Empty);

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                luceneQuery.Append(" AND TimestampUtc:[{0} TO {1}]".FormatWith(
                    DateTools.DateToString(request.StartDate.Value, DateTools.Resolution.MILLISECOND),
                    DateTools.DateToString(request.EndDate.Value, DateTools.Resolution.MILLISECOND)));
            }

            if (!request.Query.IsNullOrEmpty())
            {
                luceneQuery.Append(" AND Text:{0}".FormatWith(request.Query));
            }

            if (!request.ExceptionType.IsNullOrEmpty())
            {
                luceneQuery.Append(" AND Type:{0}".FormatWith(request.ExceptionType));
            }

            if (!request.Application.IsNullOrEmpty())
            {
                luceneQuery.Append(" AND Application:{0}".FormatWith(request.Application));
            }

            if (!request.MessageId.IsNullOrEmpty())
            {
                luceneQuery.Append(" AND MessageId:{0}".FormatWith(request.MessageId));
            }

            string whereClause = luceneQuery.ToString();

            //remove the first occurance of AND
            int firstAndInstance = whereClause.IndexOf("AND");
            if (firstAndInstance > 0)
            {
                whereClause = whereClause.Remove(firstAndInstance, 3).Trim();
            }

            var retrievedEntities = entities
                .ConditionalWhere(whereClause, whereClause.IsNotNullOrEmpty())
                .Skip((request.Paging.PageNumber - 1) * request.Paging.PageSize)
                .Take(request.Paging.PageSize)
                .AddOrder("TimestampUtc", true)
                .ToList();

            var page = new Page<ErrorditeError>(retrievedEntities, new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            return new GetErrorditeErrorsResponse
            {
                Errors = page
            };
        }
    }

    public interface IGetErrorditeErrorsQuery : IQuery<GetErrorditeErrorsRequest, GetErrorditeErrorsResponse>
    { }

    public class GetErrorditeErrorsResponse
    {
        public Page<ErrorditeError> Errors { get; set; }
    }

    public class GetErrorditeErrorsRequest
    {
        public string Query { get; set; }
        public string ExceptionType { get; set; }
        public string Application { get; set; }
        public string MessageId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PageRequestWithSort Paging { get; set; }
    }
}
