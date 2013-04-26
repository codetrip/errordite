using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Users.Queries;
using System.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Groups.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class AddGroupCommand : SessionAccessBase, IAddGroupCommand
    {
        private readonly IGetUsersQuery _getUsersQuery;
        private readonly ErrorditeConfiguration _configuration;

        public AddGroupCommand(ErrorditeConfiguration configuration, IGetUsersQuery getUsersQuery)
        {
            _configuration = configuration;
            _getUsersQuery = getUsersQuery;
        }

        public AddGroupResponse Invoke(AddGroupRequest request)
        {
            Trace("Starting...");

            var existingGroup = Session.Raven.Query<Group, Groups_Search>().FirstOrDefault(o => o.Name == request.Name);

            if (existingGroup != null)
            {
                return new AddGroupResponse(true)
                {
                    Status = AddGroupStatus.GroupExists
                };
            }

            var group = new Group
            {
                Name = request.Name,
                OrganisationId = request.Organisation.Id
            };

            Store(group);

            //update the groups users
            if (request.Users != null && request.Users.Count > 0)
            {
                foreach (var userId in request.Users)
                {
                    var user = Load<User>(User.GetId(userId));

                    if(user != null)
                        user.GroupIds.Add(group.Id);
                }
            }

            Session.SynchroniseIndexes<Groups_Search, Users_Search>();

            return new AddGroupResponse(false, request.Organisation.Id)
            {
                Status = AddGroupStatus.Ok
            };
        }
    }

    public interface IAddGroupCommand : ICommand<AddGroupRequest, AddGroupResponse>
    { }

    public class AddGroupResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;
        public AddGroupStatus Status { get; set; }

        public AddGroupResponse(bool ignoreCache, string organisationId = "")
            : base(ignoreCache)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return Caching.CacheInvalidation.GetGroupInvalidationItems(_organisationId);
        }
    }

    public class AddGroupRequest
    {
        public string Name { get; set; }
        public Organisation Organisation { get; set; }
        public IList<string> Users { get; set; }
    }

    public enum AddGroupStatus
    {
        Ok,
        GroupExists,
        PlanThresholdReached
    }
}
