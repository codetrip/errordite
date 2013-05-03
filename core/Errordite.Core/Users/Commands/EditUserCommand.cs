using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Domain.Master;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Core.Users.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class EditUserCommand : SessionAccessBase, IEditUserCommand
    {
		private readonly IAuthorisationManager _authorisationManager;
		private readonly IGetRavenInstancesQuery _getRavenInstancesQuery;

        public EditUserCommand(IAuthorisationManager authorisationManager, IGetRavenInstancesQuery getRavenInstancesQuery)
        {
	        _authorisationManager = authorisationManager;
	        _getRavenInstancesQuery = getRavenInstancesQuery;
        }

	    public EditUserResponse Invoke(EditUserRequest request)
        {
            Trace("Starting...");

            var userId = User.GetId(request.UserId);
            var existingUser = Load<User>(userId);
	        var cacheInvalidationItems = new List<CacheInvalidationItem>();

			if (existingUser == null)
			{
				return new EditUserResponse
				{
					Status = EditUserStatus.UserNotFound
				};
			}

			var userMapping = Session.MasterRaven.Query<UserOrganisationMapping>().First(u => u.EmailAddress == existingUser.Email);

			if (userMapping == null)
			{
				return new EditUserResponse
				{
					Status = EditUserStatus.UserNotFound
				};
			}

            _authorisationManager.Authorise(existingUser, request.CurrentUser);

            var existingEmail = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == request.Email && u.Id != userId);

            if (existingEmail != null)
            {
                return new EditUserResponse
                {
                    Status = EditUserStatus.EmailExists
                };
            }

	        if (existingUser.Email != request.Email)
	        {
		        userMapping.EmailAddress = request.Email;
		        Session.SynchroniseIndexes<UserOrganisationMappings>(true);
			}

			var ravenInstances = _getRavenInstancesQuery.Invoke(new GetRavenInstancesRequest()).RavenInstances;

			//find the users accounts in their organisations and sync the user
	        foreach (var organisationId in userMapping.Organisations)
			{
				var organisation = MasterLoad<Organisation>(organisationId);
				if (organisation == null)
				{
					userMapping.Organisations.Remove(organisationId);
				}
				else
				{
					organisation.RavenInstance = ravenInstances.First(r => r.Id == organisation.RavenInstanceId);

					using (Session.SwitchOrg(organisation))
					{
						var user = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == request.Email);

						if (user == null)
						{
							userMapping.Organisations.Remove(organisationId);
						}
						else
						{
							user.Email = request.Email;
							user.FirstName = request.FirstName;
							user.LastName = request.LastName;

							if (organisation.Id == request.CurrentUser.ActiveOrganisation.Id)
							{
								if (request.Administrator.HasValue && existingUser.Role != UserRole.SuperUser)
									user.Role = request.Administrator.Value ? UserRole.Administrator : UserRole.User;

								if (request.GroupIds != null)
									user.GroupIds = request.GroupIds.Select(Group.GetId).ToList();
							}

							cacheInvalidationItems.AddRange(CacheInvalidation.GetUserInvalidationItems(organisation.Id, existingUser.Email));
						}
					}
				}
			}

            Session.SynchroniseIndexes<Indexing.Users, Indexing.Groups>();

			return new EditUserResponse(cacheInvalidationItems)
            {
                Status = EditUserStatus.Ok
            };
        }
    }

    public interface IEditUserCommand : ICommand<EditUserRequest, EditUserResponse>
    { }

    public class EditUserResponse : CacheInvalidationResponseBase
    {
	    private readonly IEnumerable<CacheInvalidationItem> _cacheInvalidationItems;

		public EditUserResponse(IEnumerable<CacheInvalidationItem> cacheInvalidationItems = null)
			: base(cacheInvalidationItems == null)
	    {
		    _cacheInvalidationItems = cacheInvalidationItems;
	    }

	    public EditUserStatus Status { get; set; }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
			return _cacheInvalidationItems ?? new List<CacheInvalidationItem>();
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
    }

    public enum EditUserStatus
    {
        Ok,
        UserNotFound,
        EmailExists,
    }
}
