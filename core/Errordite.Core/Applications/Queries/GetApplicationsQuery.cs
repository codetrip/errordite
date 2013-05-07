using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using ProtoBuf;

namespace Errordite.Core.Applications.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetApplicationsQuery : SessionAccessBase, IGetApplicationsQuery
    {
        public GetApplicationsResponse Invoke(GetApplicationsRequest request)
        {
            Trace("Starting...");

            var page = GetPage<Application, Indexing.Applications, string>(request.Paging, orderByClause: a => a.Name);

            return new GetApplicationsResponse
            {
                Applications = page
            };
        }
    }

    public interface IGetApplicationsQuery : IQuery<GetApplicationsRequest, GetApplicationsResponse>
    { }

    [ProtoContract]
    public class GetApplicationsResponse
    {
        [ProtoMember(1)]
        public Page<Application> Applications { get; set; }
    }

    public class GetApplicationsRequest : CacheableRequestBase<GetApplicationsResponse>
    {
        public string OrganisationId { get; set; }
        public PageRequestWithSort Paging { get; set; }

        protected override string GetCacheKey()
        {
            return "{0}-{1}-{2}".FormatWith(CacheKeys.Applications.PerOrganisationPrefix(OrganisationId), Paging.PageSize, Paging.PageNumber);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Applications;
        }
    }
}
