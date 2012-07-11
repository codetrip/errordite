using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using CodeTrip.Core.Extensions;
using Errordite.Core.Indexing;
using ProtoBuf;

namespace Errordite.Core.Groups.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetGroupsQuery : SessionAccessBase, IGetGroupsQuery
    {
        public GetGroupsResponse Invoke(GetGroupsRequest request)
        {
            Trace("Starting...");

            var groups = GetPage<Group, Groups_Search, string>(request.Paging, u => u.OrganisationId == Organisation.GetId(request.OrganisationId), u => u.Name);

            return new GetGroupsResponse
            {
                Groups = groups
            };
        }
    }

    public interface IGetGroupsQuery : IQuery<GetGroupsRequest, GetGroupsResponse>
    { }

    [ProtoContract]
    public class GetGroupsResponse
    {
        [ProtoMember(1)]
        public Page<Group> Groups { get; set; }
    }

    public class GetGroupsRequest : CacheableRequestBase<GetGroupsResponse>
    {
        public string OrganisationId { get; set; }
        public PageRequestWithSort Paging { get; set; }

        protected override string GetCacheKey()
        {
            return "{0}-{1}-{2}".FormatWith(CacheKeys.Groups.PerOrganisationPrefix(OrganisationId), Paging.PageSize, Paging.PageNumber);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Groups;
        }
    }
}
