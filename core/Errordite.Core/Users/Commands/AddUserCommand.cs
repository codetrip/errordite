using System;
using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Domain.Master;
using Errordite.Core.Encryption;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Core.Session;
using Raven.Client;

namespace Errordite.Core.Users.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class AddUserCommand : SessionAccessBase, IAddUserCommand
    {
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly IEncryptor _encryptor;

        public AddUserCommand(IEncryptor encryptor, 
            ISendNotificationCommand sendNotificationCommand)
        {
            _encryptor = encryptor;
            _sendNotificationCommand = sendNotificationCommand;
        }

        public AddUserResponse Invoke(AddUserRequest request)
        {
            Trace("Starting...");

            var existingUserOrgMap = Session.MasterRaven
                .Query<UserOrganisationMapping, UserOrganisationMappings>()
                .FirstOrDefault(u => u.EmailAddress == request.Email);

            if (existingUserOrgMap != null && existingUserOrgMap.Organisations.Contains(request.Organisation.Id))
            {
	            return new AddUserResponse(true)
                {
                    Status = AddUserStatus.EmailExists,
                };
            }

            var existingUser = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return new AddUserResponse(true)
                {
                    Status = AddUserStatus.EmailExists
                };
            }

            RavenQueryStatistics stats;
            var users = Session.Raven.Query<User, Indexing.Users>()
                .Statistics(out stats)
                .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                .Take(0);

            if(stats.TotalResults >= request.Organisation.PaymentPlan.MaximumUsers)
            {
                return new AddUserResponse(true)
                {
                    Status = AddUserStatus.PlanThresholdReached
                };
            }

			if (existingUserOrgMap != null)
			{
				return new AddUserResponse(true)
				{
					Status = AddUserStatus.EmailExistsInAnotherOrganisation
				};

				//existingUserOrgMap.Organisations.Add(request.Organisation.Id);

				//_sendNotificationCommand.Invoke(new SendNotificationRequest
				//{
				//	EmailInfo = new NewUserEmailInfo
				//	{
				//		To = user.Email,
				//		Token = _encryptor.Encrypt("{0}|{1}".FormatWith(user.PasswordToken.ToString(), request.Organisation.FriendlyId)).Base64Encode(),
				//		UserName = user.FirstName
				//	},
				//	OrganisationId = request.Organisation.Id,
				//	Organisation = request.Organisation
				//});
			}

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = UserStatus.Inactive,
                Role = request.Administrator ? UserRole.Administrator : UserRole.User,
                OrganisationId = request.Organisation.Id,
                GroupIds = request.GroupIds.Select(Group.GetId).ToList(),
                PasswordToken = Guid.NewGuid()
            };

            Store(user);

			var userOrgMapping = new UserOrganisationMapping
			{
				EmailAddress = request.Email,
				Organisations = new List<string> { request.Organisation.Id },
				Password = user.Password,
				PasswordToken = user.PasswordToken
			};

			MasterStore(userOrgMapping);

			_sendNotificationCommand.Invoke(new SendNotificationRequest
			{
				EmailInfo = new NewUserEmailInfo
				{
					To = user.Email,
					Token = _encryptor.Encrypt("{0}|{1}".FormatWith(user.PasswordToken.ToString(), request.Organisation.FriendlyId)).Base64Encode(),
					UserName = user.FirstName
				},
				OrganisationId = request.Organisation.Id,
				Organisation = request.Organisation
			});
			

			Session.SynchroniseIndexes<Indexing.Users, Indexing.Groups>();
            Session.SynchroniseIndexes<UserOrganisationMappings>(true);

            return new AddUserResponse(false, request.Organisation.Id)
            {
                Status = AddUserStatus.Ok
            };
        }
    }

    public interface IAddUserCommand : ICommand<AddUserRequest, AddUserResponse>
    { }

    public class AddUserResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;
        public AddUserStatus Status { get; set; }

        public AddUserResponse(bool ignoreCache, string organisationId = "")
            : base(ignoreCache)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return Caching.CacheInvalidation.GetUserInvalidationItems(_organisationId);
        }
    }

    public class AddUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Organisation Organisation { get; set; }
        public IList<string> GroupIds { get; set; }
        public bool Administrator { get; set; }
    }

    public enum AddUserStatus
    {
        Ok,
        EmailExists,
        PlanThresholdReached,
        EmailExistsInAnotherOrganisation
    }
}
