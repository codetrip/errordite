using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using ProtoBuf;

namespace Errordite.Core.Groups.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetGroupsQuery : SessionAccessBase, IGetGroupsQuery
    {
        public GetGroupsResponse Invoke(GetGroupsRequest request)
        {
            Trace("Starting...");

            var groups = GetPage<Group, Indexing.Groups, string>(request.Paging, orderByClause: u => u.Name);

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
