﻿using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using CodeTrip.Core.Extensions;
using Errordite.Core.Groups.Queries;
using Errordite.Core.Indexing;
using ProtoBuf;
using Raven.Client.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Users.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetUsersQuery : SessionAccessBase, IGetUsersQuery
    {
        private readonly IGetGroupsQuery _getGroupsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public GetUsersQuery(IGetGroupsQuery getGroupsQuery, ErrorditeConfiguration configuration)
        {
            _getGroupsQuery = getGroupsQuery;
            _configuration = configuration;
        }

        public GetUsersResponse Invoke(GetUsersRequest request)
        {
            Trace("Starting...");

            Raven.Client.RavenQueryStatistics stats;

            var query = Session.Raven.Query<User, Users_Search>()
                .Statistics(out stats)
                .Where(e => e.OrganisationId == Organisation.GetId(request.OrganisationId))
                .OrderBy(u => u.LastName);

            var users = new Page<User>(query.ToList(), new PagingStatus(request.Paging.PageSize, request.Paging.PageNumber, stats.TotalResults));

            var groups = _getGroupsQuery.Invoke(new GetGroupsRequest
            {
                OrganisationId = request.OrganisationId,
                Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
            }).Groups;

            foreach (var user in users.Items)
            {
                user.Groups = groups.Items.Where(g => user.GroupIds.Contains(g.Id)).ToList();
            }

            return new GetUsersResponse
            {
                Users = users
            };
        }
    }

    public interface IGetUsersQuery : IQuery<GetUsersRequest, GetUsersResponse>
    { }

    [ProtoContract]
    public class GetUsersResponse
    {
        [ProtoMember(1)]
        public Page<User> Users { get; set; }
    }

    public class GetUsersRequest : CacheableRequestBase<GetUsersResponse>
    {
        public string OrganisationId { get; set; }
        public PageRequestWithSort Paging { get; set; }

        protected override string GetCacheKey()
        {
            return "{0}-{1}-{2}".FormatWith(CacheKeys.Users.PerOrganisationPrefix(OrganisationId), Paging.PageSize, Paging.PageNumber);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Users;
        }
    }
}
