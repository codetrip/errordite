using System;
using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using CodeTrip.Core.Extensions;
using System.Linq;
using Errordite.Core.Indexing;
using Errordite.Core.Matching;
using Errordite.Core.Organisations.Queries;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class CreateOrganisationCommand : SessionAccessBase, ICreateOrganisationCommand
    {
        private readonly IGetPaymentPlansQuery _getPaymentPlansQuery;
        private readonly IAddApplicationCommand _addApplicationCommand;

        public CreateOrganisationCommand(IGetPaymentPlansQuery getPaymentPlansQuery, IAddApplicationCommand addApplicationCommand)
        {
            _getPaymentPlansQuery = getPaymentPlansQuery;
            _addApplicationCommand = addApplicationCommand;
        }

        public CreateOrganisationResponse Invoke(CreateOrganisationRequest request)
        {
            Trace("Starting...");

            var existingOrganisation = Session.Raven.Query<Organisation, Organisations_Search>().FirstOrDefault(o => o.Name == request.OrganisationName);

            if(existingOrganisation != null)
            {
                return new CreateOrganisationResponse
                {
                    Status = CreateOrganisationStatus.OrganisationExists
                };
            } 
            
            var existingUser = Session.Raven.Query<User, Users_Search>().FirstOrDefault(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return new CreateOrganisationResponse
                {
                    Status = CreateOrganisationStatus.UserExists
                };
            }

            var freeTrialPlan = _getPaymentPlansQuery.Invoke(new GetPaymentPlansRequest()).Plans.First(p => p.PlanType == PaymentPlanType.Trial);

            var organisation = new Organisation
            {
                Name = request.OrganisationName,
                Status = OrganisationStatus.Active,
                PaymentPlanId = freeTrialPlan.Id,
                CreatedOnUtc = DateTime.UtcNow,
                TimezoneId = "UTC",
                PaymentPlan = freeTrialPlan
            };

            Store(organisation);

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
                Password = request.Password.Hash(),
                OrganisationId = organisation.Id,
                Role = UserRole.Administrator,
                Status = UserStatus.Active,
                GroupIds = new List<string> { group.Id },
                Organisation = organisation
            };

            Store(user);

            var addApplicationResponse = _addApplicationCommand.Invoke(new AddApplicationRequest
            {
                CurrentUser = user,
                IsActive = true,
                MatchRuleFactoryId = new MethodAndTypeMatchRuleFactory().Id,
                Name = request.OrganisationName,
                Notifications = Enum.GetValues(typeof (NotificationType)).Cast<NotificationType>()
                    .Select(n =>
                        new Notification
                        {
                            Groups = new List<string> {group.Id}, 
                            Type = n
                        }).ToList(),
                UserId = user.Id,
            });

            return new CreateOrganisationResponse
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
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        public CreateOrganisationStatus Status { get; set; }

        public string ApplicationId { get; set; }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            yield return new CacheInvalidationItem(CacheProfiles.Organisations, CacheKeys.Organisations.Key());
        }
    }

    public class CreateOrganisationRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string OrganisationName { get; set; }
    }

    public enum CreateOrganisationStatus
    {
        Ok,
        UserExists,
        OrganisationExists
    }
}
