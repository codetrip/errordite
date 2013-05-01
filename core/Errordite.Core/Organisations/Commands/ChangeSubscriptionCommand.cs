using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Castle.Core;
using ChargifyNET;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Encryption;
using Errordite.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Session;
using System.Linq;
using Errordite.Core.Session.Actions;

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

            subscription = connection.CreateSubscription(request.NewPlanName.ToLowerInvariant(), subscription.Customer.ChargifyID);

            organisation.PaymentPlanId = PaymentPlan.GetId(request.NewPlanId);
            organisation.Subscription.ChargifyId = subscription.SubscriptionID;
            organisation.Subscription.Status = SubscriptionStatus.Active;

            Session.SynchroniseIndexes<Indexing.Organisations, Indexing.Users>();
			//Session.AddCommitAction(new SendMessageCommitAction(
			//	new SignUpCompleteEmailInfo
			//	{
			//		OrganisationName = organisation.Name,
			//		SubscriptionId = request.SubscriptionId.ToString(),
			//		UserName = request.CurrentUser.FirstName,
			//		BillingAmount = string.Format(CultureInfo.GetCultureInfo(1033), "{0:C}", organisation.PaymentPlan.Price)
			//	},
			//	_configuration.GetNotificationsQueueAddress(organisation.RavenInstanceId)));

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
    }

    public enum ChangeSubscriptionStatus
    {
        Ok,
        OrganisationNotFound,
        SubscriptionNotFound,
		ChangelationFailed
    }
}
