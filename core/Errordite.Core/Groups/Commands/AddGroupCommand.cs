using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Users.Queries;
using System.Linq;

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

            var existingGroup = Session.Raven.Query<Group, Groups_Search>().FirstOrDefault(o => o.Name == request.Name && o.OrganisationId == request.Organisation.Id);

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
                var currentUsers = _getUsersQuery.Invoke(new GetUsersRequest
                {
                    OrganisationId = request.Organisation.Id,
                    Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
                }).Users;

                foreach (var user in currentUsers.Items)
                {
                    if (request.Users.Any(u => u == user.Id))
                    {
                        user.GroupIds.Add(group.Id);
                        Store(user); //does not seem to update here without calling store
                    }
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
