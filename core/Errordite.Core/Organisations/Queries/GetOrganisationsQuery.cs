using System.Linq;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using ProtoBuf;
using Raven.Client;
using Raven.Client.Linq;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetOrganisationsQuery : SessionAccessBase, IGetOrganisationsQuery
    {
        public GetOrganisationsResponse Invoke(GetOrganisationsRequest request)
        {
            Trace("Starting...");

			RavenQueryStatistics stats;

            var entities = Session.MasterRaven.Query<OrganisationDocument, Indexing.Organisations>()
				.Statistics(out stats)
				.Skip((request.Paging.PageNumber - 1) * request.Paging.PageSize)
				.Take(request.Paging.PageSize);

			var page = new Page<Organisation>(entities.As<Organisation>().ToList(), new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            return new GetOrganisationsResponse
            {
                Organisations = page
            };
        }
    }

    public interface IGetOrganisationsQuery : IQuery<GetOrganisationsRequest, GetOrganisationsResponse>
    { }

    [ProtoContract]
    public class GetOrganisationsResponse
    {
        [ProtoMember(1)]
        public Page<Organisation> Organisations { get; set; }
    }

    public class GetOrganisationsRequest : CacheableRequestBase<GetOrganisationsResponse>
    {
        public PageRequestWithSort Paging { get; set; }

        protected override string GetCacheKey()
        {
            return CacheKeys.Organisations.Key();
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Organisations;
        }
    }
}
