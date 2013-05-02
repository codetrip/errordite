using System;
using System.Collections.Generic;
using System.Globalization;
using Castle.Core;
using ChargifyNET;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Session;
using System.Linq;
using Errordite.Core.Session.Actions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class  ChangeSubscriptionCommand : SessionAccessBase, IChangeSubscriptionCommand
    {
        private readonly ErrorditeConfiguration _configuration;

        public ChangeSubscriptionCommand(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ChangeSubscriptionResponse Invoke(ChangeSubscriptionRequest request)
        {
			Trace("Starting...");

			var organisation = Session.MasterRaven.Load<Organisation>(request.CurrentUser.Organisation.Id);

			if (organisation == null)
			{
				return new ChangeSubscriptionResponse(ignoreCache: true)
				{
					Status = ChangeSubscriptionStatus.OrganisationNotFound
				};
			}

			if (!organisation.Subscription.ChargifyId.HasValue)
			{
				return new ChangeSubscriptionResponse(ignoreCache: true)
				{
					Status = ChangeSubscriptionStatus.SubscriptionNotFound
				};
			}  

            var connection = new ChargifyConnect(_configuration.ChargifyUrl, _configuration.ChargifyApiKey, _configuration.ChargifyPassword);
			var subscription = connection.LoadSubscription(organisation.Subscription.ChargifyId.Value);

            if (subscription == null)
            {
                return new ChangeSubscriptionResponse(ignoreCache:true)
                {
                    Status = ChangeSubscriptionStatus.SubscriptionNotFound
                };
            }

			subscription = connection.EditSubscriptionProduct(organisation.Subscription.ChargifyId.Value, request.NewPlanName.ToLowerInvariant());

            organisation.PaymentPlanId = PaymentPlan.GetId(request.NewPlanId);

	        var plan = MasterLoad<PaymentPlan>(organisation.PaymentPlanId);

            organisation.Subscription.ChargifyId = subscription.SubscriptionID;
            organisation.Subscription.Status = SubscriptionStatus.Active;
			organisation.Subscription.LastModified = DateTime.UtcNow.ToDateTimeOffset(organisation.TimezoneId);

            Session.SynchroniseIndexes<Indexing.Organisations, Indexing.Users>();
			Session.AddCommitAction(new SendMessageCommitAction(
				new SubsciptionChangedEmailInfo
				{
					OrganisationName = organisation.Name,
					SubscriptionId = subscription.SubscriptionID.ToString(),
					UserName = request.CurrentUser.FirstName,
					BillingAmount = string.Format(CultureInfo.GetCultureInfo(1033), "{0:C}", plan.Price),
					BillingPeriodEndDate = organisation.Subscription.CurrentPeriodEndDate.ToLocalFormatted(),
					OldPlanName = request.OldPlanName,
					NewPlanName = plan.Name
				},
				_configuration.GetNotificationsQueueAddress(organisation.RavenInstanceId)));

            return new ChangeSubscriptionResponse(organisation.Id, request.CurrentUser.Id, request.CurrentUser.Email)
            {
                Status = ChangeSubscriptionStatus.Ok
            };
        }
    }

    public interface IChangeSubscriptionCommand : ICommand<ChangeSubscriptionRequest, ChangeSubscriptionResponse>
    { }

    public class ChangeSubscriptionResponse : CacheInvalidationResponseBase
    {
	    private readonly string _organisationId;
		private readonly string _userId;
		private readonly string _email;
        public ChangeSubscriptionStatus Status { get; set; }

        public ChangeSubscriptionResponse(string organisationId = null, string userId = null, string email = null, bool ignoreCache = false)
            : base(ignoreCache)
        {
            _organisationId = organisationId;
            _userId = userId;
            _email = email;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetOrganisationInvalidationItems(_organisationId, _email).Union(
                   CacheInvalidation.GetUserInvalidationItems(_organisationId, _userId, _email));
        }
    }

    public class ChangeSubscriptionRequest : OrganisationRequestBase
    {
        public string NewPlanId { get; set; }
		public string NewPlanName { get; set; }
		public string OldPlanName { get; set; }
    }

    public enum ChangeSubscriptionStatus
    {
        Ok,
        OrganisationNotFound,
        SubscriptionNotFound,
		ChangelationFailed
    }
}
