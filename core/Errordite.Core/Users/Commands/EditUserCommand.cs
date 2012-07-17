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

namespace Errordite.Core.Users.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class EditUserCommand : SessionAccessBase, IEditUserCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public EditUserCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public EditUserResponse Invoke(EditUserRequest request)
        {
            Trace("Starting...");

            var userId = User.GetId(request.UserId);
            var existingUser = Load<User>(userId);

            if (existingUser == null)
            {
                return new EditUserResponse(true)
                {
                    Status = EditUserStatus.UserNotFound
                };
            }

            _authorisationManager.Authorise(existingUser, request.CurrentUser);

            var existingEmail = Session.Raven.Query<User, Users_Search>().FirstOrDefault(u => u.Email == request.Email && u.Id != userId);

            if (existingEmail != null)
            {
                return new EditUserResponse(true)
                {
                    Status = EditUserStatus.EmailExists
                };
            }

            existingUser.FirstName = request.FirstName;
            existingUser.LastName = request.LastName;
            existingUser.Email = request.Email.ToLowerInvariant(); //need to do this so email is not case sensitive when signing in
            existingUser.TimezoneId = request.TimezoneId;

            if (request.Administrator.HasValue && existingUser.Role != UserRole.SuperUser)
                existingUser.Role = request.Administrator.Value ? UserRole.Administrator : UserRole.User;

            if (request.GroupIds != null)
                existingUser.GroupIds = request.GroupIds.Select(Group.GetId).ToList();

            Session.SynchroniseIndexes<Users_Search, Groups_Search>();

            return new EditUserResponse(false, request.UserId, request.CurrentUser.OrganisationId)
            {
                Status = EditUserStatus.Ok
            };
        }
    }

    public interface IEditUserCommand : ICommand<EditUserRequest, EditUserResponse>
    { }

    public class EditUserResponse : CacheInvalidationResponseBase
    {
        private readonly string _userId;
        private readonly string _organisationId;

        public EditUserResponse(bool ignoreCache, string userId = "", string organisationId = "")
            : base(ignoreCache)
        {
            _userId = userId;
            _organisationId = organisationId;
        }

        public EditUserStatus Status { get; set; }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return Caching.CacheInvalidation.GetUserInvalidationItems(_organisationId, _userId);
        }
    }

    public class EditUserRequest : OrganisationRequestBase
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool? Administrator { get; set; }
        public IList<string> GroupIds { get; set; }
        public string TimezoneId { get; set; }
    }

    public enum EditUserStatus
    {
        Ok,
        UserNotFound,
        EmailExists,
    }
}
