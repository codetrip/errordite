using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Authorisation;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using CodeTrip.Core.Extensions;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Users.Queries;
using System.Linq;

namespace Errordite.Core.Groups.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class EditGroupCommand : SessionAccessBase, IEditGroupCommand
    {
        private readonly IGetUsersQuery _getUsersQuery;
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;
        
        public EditGroupCommand(IGetUsersQuery getUsersQuery, 
            ErrorditeConfiguration configuration, 
            IAuthorisationManager authorisationManager)
        {
            _getUsersQuery = getUsersQuery;
            _configuration = configuration;
            _authorisationManager = authorisationManager;
        }

        public EditGroupResponse Invoke(EditGroupRequest request)
        {
            Trace("Starting...");

            var groupId = Group.GetId(request.GroupId);
            var existingGroup = Load<Group>(groupId);

            if (existingGroup == null)
            {
                return new EditGroupResponse(true)
                {
                    Status = EditGroupStatus.GroupNotFound
                };
            }

            _authorisationManager.Authorise(existingGroup, request.CurrentUser);

            //update the groups users
            var currentUsers = _getUsersQuery.Invoke(new GetUsersRequest
            {
                OrganisationId = request.CurrentUser.OrganisationId,
                Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
            }).Users;

            foreach (var user in currentUsers.Items)
            {
                if (request.Users.Any(u => u == user.Id) && !user.GroupIds.Any(gId => gId == existingGroup.Id))
                {
                    user.GroupIds.Add(existingGroup.Id);
                    Store(user); //does not seem to update here without calling store
                }
                else if (!request.Users.Any(u => u == user.Id) && user.GroupIds.Any(gId => gId == existingGroup.Id))
                {
                    user.GroupIds.Remove(existingGroup.Id);
                    Store(user); //does not seem to update here without calling store
                }
            }

            if (request.Name.IsNotNullOrEmpty())
                existingGroup.Name = request.Name;

            Session.SynchroniseIndexes<Users_Search, Groups_Search>();

            return new EditGroupResponse(false, request.GroupId, request.CurrentUser.OrganisationId)
            {
                Status = EditGroupStatus.Ok
            };
        }
    }

    public interface IEditGroupCommand : ICommand<EditGroupRequest, EditGroupResponse>
    { }

    public class EditGroupResponse : CacheInvalidationResponseBase
    {
        private readonly string _groupId;
        private readonly string _organisationId;
        public EditGroupStatus Status { get; set; }

        public EditGroupResponse(bool ignoreCache, string groupId = "", string organisationId = "")
            : base(ignoreCache)
        {
            _groupId = groupId;
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return Caching.CacheInvalidation.GetGroupInvalidationItems(_organisationId, _groupId);
        }
    }

    public class EditGroupRequest : OrganisationRequestBase
    {
        public string GroupId { get; set; }
        public string Name { get; set; }
        public IList<string> Users { get; set; }
    }

    public enum EditGroupStatus
    {
        Ok,
        GroupNotFound
    }
}
