using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using ProtoBuf;

namespace Errordite.Core.Applications.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetApplicationsQuery : SessionAccessBase, IGetApplicationsQuery
    {
        public GetApplicationsResponse Invoke(GetApplicationsRequest request)
        {
            Trace("Starting...");

            var organisationId = Organisation.GetId(request.OrganisationId);
            var page = GetPage<Application, Applications_Search, string>(request.Paging, a => a.OrganisationId == organisationId, a => a.Name);

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
