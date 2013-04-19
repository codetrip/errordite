
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

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetOrganisationsQuery : SessionAccessBase, IGetOrganisationsQuery
    {
        public GetOrganisationsResponse Invoke(GetOrganisationsRequest request)
        {
            Trace("Starting...");

            var page = GetMasterPage<Organisation, Organisations_Search, string>(request.Paging);

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
