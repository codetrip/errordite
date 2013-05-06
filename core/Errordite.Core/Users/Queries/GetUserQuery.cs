using System.Collections.Generic;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Groups.Queries;
using System.Linq;
using Errordite.Core.Session;

namespace Errordite.Core.Users.Queries
{
    public class GetUserQuery : SessionAccessBase, IGetUserQuery
    {
        private readonly IGetGroupsQuery _getGroupsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public GetUserQuery(IGetGroupsQuery getGroupsQuery, ErrorditeConfiguration configuration)
        {
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
                if(user.GroupIds != null && user.GroupIds.Count > 0)
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

            return user == null ? null : new GetUserResponse
            {
                User = user
            };
        }
    }

    public interface IGetUserQuery : IQuery<GetUserRequest, GetUserResponse>
    { }

    public class GetUserResponse
    {
        public User User { get; set; }
    }

    public class GetUserRequest
    {
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
    }
}
