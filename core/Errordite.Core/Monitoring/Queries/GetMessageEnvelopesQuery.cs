using System.Linq;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Interfaces;
using Errordite.Core.Messaging;
using Errordite.Core.Paging;
using Errordite.Core.Session;
using Raven.Client;
using Raven.Client.Linq;
using Errordite.Core.Extensions;

namespace Errordite.Core.Monitoring.Queries
{
	public class GetMessageEnvelopesQuery : SessionAccessBase, IGetMessageEnvelopes
    {
		public GetMessageEnvelopesResponse Invoke(GetMessageEnvelopesRequest request)
		{
			RavenQueryStatistics stats;

			var query = Session.MasterRaven.Query<MessageEnvelope, MessageEnvelopes>().Statistics(out stats);

			if (request.Service.HasValue)
			{
				query = query.Where(i => i.Service == request.Service);
			}

			if (request.OrganisationId.IsNotNullOrEmpty())
			{
				query = query.Where(e => e.OrganisationId == Organisation.GetId(request.OrganisationId));
			}

			var envelopes = query
				.Skip((request.Paging.PageNumber - 1) * request.Paging.PageSize)
				.Take(request.Paging.PageSize)
				.OrderByDescending(e => e.GeneratedOnUtc);

			return new GetMessageEnvelopesResponse
			{
				Envelopes = new Page<MessageEnvelope>(envelopes.ToList(), new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults))
			};
		}
    }

	public interface IGetMessageEnvelopes : IQuery<GetMessageEnvelopesRequest, GetMessageEnvelopesResponse>
    { }

	public class GetMessageEnvelopesResponse
    {
        public Page<MessageEnvelope> Envelopes { get; set; }
    }

	public class GetMessageEnvelopesRequest
    {
        public Service? Service { get; set; }
		public string OrganisationId { get; set; }
		public PageRequestWithSort Paging { get; set; }
    }
}
