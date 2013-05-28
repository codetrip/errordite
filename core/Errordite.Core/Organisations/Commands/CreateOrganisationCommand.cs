using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Domain.Master;
using Errordite.Core.Encryption;
using Errordite.Core.Interfaces;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using System.Linq;
using Errordite.Core.Indexing;
using Errordite.Core.Matching;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class  CreateOrganisationCommand : SessionAccessBase, ICreateOrganisationCommand
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;
        private readonly IAddApplicationCommand _addApplicationCommand;
        private readonly IEncryptor _encryptor;
        private readonly IGetRavenInstancesQuery _getRavenInstancesQuery;

        public CreateOrganisationCommand(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, 
            IAddApplicationCommand addApplicationCommand, 
            IEncryptor encryptor, 
            IGetRavenInstancesQuery getRavenInstancesQuery)
        {
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
            _addApplicationCommand = addApplicationCommand;
            _encryptor = encryptor;
            _getRavenInstancesQuery = getRavenInstancesQuery;
        }

        public CreateOrganisationResponse Invoke(CreateOrganisationRequest request)
        {
            Trace("Starting...");

            var existingOrganisation = Session.MasterRaven
				.Query<Organisation, Indexing.Organisations>()
				.FirstOrDefault(o => o.Name == request.OrganisationName);

            if(existingOrganisation != null)
            {
                return new CreateOrganisationResponse
                {
                    Status = CreateOrganisationStatus.OrganisationExists
                };
            }

            var freeTrialPlan = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans.First(p => p.IsFreeTier && p.IsAvailable);
	        var timezone = request.TimezoneId ?? "UTC";
	        var date = DateTime.UtcNow.ToDateTimeOffset(timezone);

            var organisation = new Organisation
            {
                Name = request.OrganisationName,
                Status = OrganisationStatus.Active,
                PaymentPlanId = freeTrialPlan.Id,
                CreatedOnUtc = DateTime.UtcNow,
				TimezoneId = timezone,
                PaymentPlan = freeTrialPlan,
                ApiKeySalt = Membership.GeneratePassword(8, 1),
				Subscription = new Subscription
				{
					Status = SubscriptionStatus.Trial,
					StartDate = date,
					CurrentPeriodEndDate = date.AddMonths(1),
					LastModified = date
				}
            };

            var ravenInstance = _getRavenInstancesQuery.Invoke(new GetRavenInstancesRequest())
                .RavenInstances
                .FirstOrDefault(r => r.Active) ?? RavenInstance.Master();

            organisation.RavenInstance = ravenInstance;
            organisation.RavenInstanceId = ravenInstance.Id;

            MasterStore(organisation);

			var existingUserOrgMap = Session.MasterRaven
				.Query<UserOrganisationMapping, UserOrganisationMappings>()
				.FirstOrDefault(u => u.EmailAddress == request.Email);

			if (existingUserOrgMap != null)
			{
				existingUserOrgMap.Organisations.Add(organisation.Id);
			}
			else
			{
				MasterStore(new UserOrganisationMapping
				{
					EmailAddress = request.Email,
					Organisations = new List<string>{ organisation.Id },
					Password = request.Password.Hash(),
					PasswordToken = Guid.Empty,
					Status = UserStatus.Active,
				});
			}

            organisation.ApiKey = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                _encryptor.Encrypt("{0}|{1}".FormatWith(organisation.FriendlyId, organisation.ApiKeySalt))));

            Session.SetOrganisation(organisation);
            Session.BootstrapOrganisation(organisation);

            var group = new Group
            {
                Name = request.OrganisationName,
                OrganisationId = organisation.Id
            };

            Store(group);

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.Administrator,
                GroupIds = new List<string> { group.Id },
                ActiveOrganisation = organisation,
				OrganisationId = organisation.Id,
            };

            Store(user);

            //update the organisation with the primary user
            organisation.PrimaryUserId = user.Id;

            var addApplicationResponse = _addApplicationCommand.Invoke(new AddApplicationRequest
            {
                CurrentUser = user,
                IsActive = true,
                MatchRuleFactoryId = new MethodAndTypeMatchRuleFactory().Id,
                Name = request.OrganisationName,
                NotificationGroups = new List<string> { group.Id },
                UserId = user.Id,
				IsSignUp = true,
            });

            //TODO: sync indexes
            Session.SynchroniseIndexes<Indexing.Organisations, Indexing.Users>();

            return new CreateOrganisationResponse(request.Email)
            {
                OrganisationId = organisation.Id,
                UserId = user.Id,
                ApplicationId = addApplicationResponse.ApplicationId,
            };
        }
    }

    public interface ICreateOrganisationCommand : ICommand<CreateOrganisationRequest, CreateOrganisationResponse>
    { }

    public class CreateOrganisationResponse : CacheInvalidationResponseBase
    {
        private readonly string _email;

        public CreateOrganisationResponse(string email = null)
        {
            _email = email;
        }

        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }
        public CreateOrganisationStatus Status { get; set; }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            yield return new CacheInvalidationItem(CacheProfiles.Organisations, CacheKeys.Organisations.Key());

            if(_email != null)
                yield return new CacheInvalidationItem(CacheProfiles.Organisations, CacheKeys.Organisations.Email(_email));
        }
    }

    public class CreateOrganisationRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string OrganisationName { get; set; }
        public string TimezoneId { get; set; }
    }

    public enum CreateOrganisationStatus
    {
        Ok,
        UserExists,
        OrganisationExists
    }
}
