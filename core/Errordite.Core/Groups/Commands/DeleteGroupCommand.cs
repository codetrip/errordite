using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Groups.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class DeleteGroupCommand : SessionAccessBase, IDeleteGroupCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public DeleteGroupCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public DeleteGroupResponse Invoke(DeleteGroupRequest request)
        {
            Trace("Starting...");

            var groupId = Group.GetId(request.GroupId);

            var existingGroup = Session.Raven.Query<Group, Groups_Search>().FirstOrDefault(o => o.Id == groupId);

            if (existingGroup == null)
            {
                return new DeleteGroupResponse(true)
                {
                    Status = DeleteGroupStatus.GroupNotFound
                };
            }

            _authorisationManager.Authorise(existingGroup, request.CurrentUser);

            var usersInGroup = Session.Raven.Query<User, Users_Search>().FirstOrDefault(e => e.GroupIds.Any(id => id == existingGroup.Id));

            if (usersInGroup != null)
            {
                return new DeleteGroupResponse(true)
                {
                    Status = DeleteGroupStatus.UsersInGroup
                };
            }

            Delete(existingGroup);

            Session.SynchroniseIndexes<Groups_Search>();

            return new DeleteGroupResponse(groupId: request.GroupId, organisationId: request.CurrentUser.OrganisationId)
            {
                Status = DeleteGroupStatus.Ok
            };
        }
    }

    public interface IDeleteGroupCommand : ICommand<DeleteGroupRequest, DeleteGroupResponse>
    { }

    public class DeleteGroupResponse : CacheInvalidationResponseBase
    {
        private readonly string _groupId;
        private readonly string _organisationId;
        public DeleteGroupStatus Status { get; set; }

        public DeleteGroupResponse(bool ignoreCache = false, string groupId = "", string organisationId = "")
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

    public class DeleteGroupRequest : OrganisationRequestBase
    {
        public string GroupId { get; set; }
    }

    public enum DeleteGroupStatus
    {
        Ok,
        UsersInGroup,
        GroupNotFound
    }
}
