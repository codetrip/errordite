using System;
using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Groups.Queries;
using Errordite.Core.Organisations.Queries;
using System.Linq;
using Errordite.Core.Session;
using ProtoBuf;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Users.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetUserQuery : SessionAccessBase, IGetUserQuery
    {
        private readonly IGetOrganisationQuery _getOrganisationQuery;
        private readonly IGetGroupsQuery _getGroupsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public GetUserQuery(IGetOrganisationQuery getOrganisationQuery, IGetGroupsQuery getGroupsQuery, ErrorditeConfiguration configuration)
        {
            _getOrganisationQuery = getOrganisationQuery;
            _getGroupsQuery = getGroupsQuery;
            _configuration = configuration;
        }

        public GetUserResponse Invoke(GetUserRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.UserId, "request.UserId");
            ArgumentValidation.NotEmpty(request.OrganisationId, "request.OrganisationId");

            var userId = User.GetId(request.UserId);
            var user = Load<User>(userId);

            if (user != null)
            {
                if (user.OrganisationId != Organisation.GetId(request.OrganisationId))
                {
                    throw new UnexpectedOrganisationIdException(user.OrganisationId, Organisation.GetId(request.OrganisationId));
                }

                user.Organisation = _getOrganisationQuery.Invoke(new GetOrganisationRequest { OrganisationId = user.OrganisationId }).Organisation;

                if(user.GroupIds != null && user.GroupIds.Count > 0)
                {
                    var groups = _getGroupsQuery.Invoke(new GetGroupsRequest
                    {
                        OrganisationId = user.OrganisationId,
                        Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
                    }).Groups;

                    user.Groups = groups.Items.Where(g => user.GroupIds.Contains(g.Id)).ToList();
                }
                else
                {
                    user.GroupIds = new List<string>();
                    user.Groups = new List<Group>();
                }
            }

            return new GetUserResponse
            {
                User = user
            };
        }
    }

    public class UnexpectedOrganisationIdException : Exception
    {
        public UnexpectedOrganisationIdException(string organisationId, string expectedId)
            :base("Loaded entity from org {0} but expected to be from {1}".FormatWith(organisationId, expectedId))
        {
            
        }
    }

    public interface IGetUserQuery : IQuery<GetUserRequest, GetUserResponse>
    { }

    [ProtoContract]
    public class GetUserResponse
    {
        [ProtoMember(1)]
        public User User { get; set; }
    }

    public class GetUserRequest : CacheableRequestBase<GetUserResponse>
    {
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        
        protected override string GetCacheKey()
        {
            return CacheKeys.Users.Key(OrganisationId, UserId);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Users;
        }
    }
}
