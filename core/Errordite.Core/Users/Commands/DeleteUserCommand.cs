using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Raven.Abstractions.Data;

namespace Errordite.Core.Users.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class DeleteUserCommand : SessionAccessBase, IDeleteUserCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public DeleteUserCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public DeleteUserResponse Invoke(DeleteUserRequest request)
        {
            Trace("Starting...");

            var userId = User.GetId(request.UserId);
            var existingUser = Load<User>(userId);

            if (existingUser == null)
            {
                return new DeleteUserResponse(true)
                {
                    Status = DeleteUserStatus.UserNotFound
                };
            }

            var userOrgMapping =
                Session.MasterRaven.Query<UserOrganisationMapping>().FirstOrDefault(u => u.EmailAddress == existingUser.Email);

            if (userOrgMapping != null)
                Session.MasterRaven.Delete(userOrgMapping);

            _authorisationManager.Authorise(existingUser, request.CurrentUser);

			Session.RavenDatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Issues,
                new IndexQuery
                {
                    Query = "UserId:{0}".FormatWith(userId)
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "UserId",
                        Type = PatchCommandType.Set,
                        Value = User.GetId(request.CurrentUser.Id)
                    }
                });

            Delete(existingUser);

            Session.SynchroniseIndexes<Users_Search, Issues_Search>();

            return new DeleteUserResponse(userId: request.UserId, organisationId: request.CurrentUser.OrganisationId, email: existingUser.Email)
            {
                Status = DeleteUserStatus.Ok
            };
        }
    }

    public interface IDeleteUserCommand : ICommand<DeleteUserRequest, DeleteUserResponse>
    { }

    public class DeleteUserResponse : CacheInvalidationResponseBase
    {
        private readonly string _userId;
        private readonly string _email;
        private readonly string _organisationId;
        public DeleteUserStatus Status { get; set; }

        public DeleteUserResponse(bool ignoreCache = false, string userId = "", string organisationId = "", string email = "")
            : base(ignoreCache)
        {
            _userId = userId;
            _organisationId = organisationId;
            _email = email;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetUserInvalidationItems(_organisationId, _userId, _email);
        }
    }

    public class DeleteUserRequest : OrganisationRequestBase
    {
        public string UserId { get; set; }
    }

    public enum DeleteUserStatus
    {
        Ok,
        UserNotFound
    }
}
