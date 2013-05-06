using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Groups.Queries;
using Errordite.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Paging;
using Errordite.Core.Session;
using System.Linq;
using ProtoBuf;

namespace Errordite.Core.Users.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetUserByEmailAddressQuery : SessionAccessBase, IGetUserByEmailAddressQuery
	{
		private readonly IGetGroupsQuery _getGroupsQuery;
		private readonly ErrorditeConfiguration _configuration;

	    public GetUserByEmailAddressQuery(IGetGroupsQuery getGroupsQuery, ErrorditeConfiguration configuration)
	    {
		    _getGroupsQuery = getGroupsQuery;
		    _configuration = configuration;
	    }

	    public GetUserByEmailAddressResponse Invoke(GetUserByEmailAddressRequest request)
        {
            var user = Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == request.EmailAddress);

			if (user != null)
			{
				if (user.GroupIds != null && user.GroupIds.Count > 0)
				{
					var groups = _getGroupsQuery.Invoke(new GetGroupsRequest
					{
						OrganisationId = request.OrganisationId,
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

            return new GetUserByEmailAddressResponse
            {
                User = user,
            };
        }
    }

    public interface IGetUserByEmailAddressQuery : IQuery<GetUserByEmailAddressRequest, GetUserByEmailAddressResponse>
    { }

    public class GetUserByEmailAddressRequest : CacheableRequestBase<GetUserByEmailAddressResponse>
    {
		public string EmailAddress { get; set; }
		public string OrganisationId { get; set; }

        protected override string GetCacheKey()
        {
			return CacheKeys.Users.Email(OrganisationId, EmailAddress);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Users;
        }
    }

    [ProtoContract]
    public class GetUserByEmailAddressResponse
    {
        [ProtoMember(1)]
        public User User { get; set; }
    }

}