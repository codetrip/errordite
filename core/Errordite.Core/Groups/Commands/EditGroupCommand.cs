using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Authorisation;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
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
                if (request.Users.Any(u => u == user.Id) && user.GroupIds.All(gId => gId != existingGroup.Id))
                {
                    var loadedUser = Load<User>(User.GetId(user.Id));
                    loadedUser.GroupIds.Add(existingGroup.Id);
                }
                else if (request.Users.All(u => u != user.Id) && user.GroupIds.Any(gId => gId == existingGroup.Id))
                {
                    var loadedUser = Load<User>(User.GetId(user.Id));
                    loadedUser.GroupIds.Remove(existingGroup.Id);
                }
            }

            if (request.Name.IsNotNullOrEmpty())
                existingGroup.Name = request.Name;

            Session.SynchroniseIndexes<Indexing.Users, Indexing.Groups>();

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
