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
    public class  CancelSubscriptionCommand : SessionAccessBase, ICancelSubscriptionCommand
    {
        private readonly ErrorditeConfiguration _configuration;

        public CancelSubscriptionCommand(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CancelSubscriptionResponse Invoke(CancelSubscriptionRequest request)
        {
			Trace("Starting...");

			var organisation = Session.MasterRaven.Load<Organisation>(request.CurrentUser.Organisation.Id);

			if (organisation == null)
			{
				return new CancelSubscriptionResponse(ignoreCache: true)
				{
					Status = CancelSubscriptionStatus.OrganisationNotFound
				};
			}

			if (!organisation.Subscription.ChargifyId.HasValue)
			{
				return new CancelSubscriptionResponse(ignoreCache: true)
				{
					Status = CancelSubscriptionStatus.SubscriptionNotFound
				};
			}  

            var connection = new ChargifyConnect(_configuration.ChargifyUrl, _configuration.ChargifyApiKey, _configuration.ChargifyPassword);
			var subscription = connection.LoadSubscription(organisation.Subscription.ChargifyId.Value);

            if (subscription == null)
            {
                return new CancelSubscriptionResponse(ignoreCache:true)
                {
                    Status = CancelSubscriptionStatus.SubscriptionNotFound
                };
            }   

	        var cancelled = connection.DeleteSubscription(organisation.Subscription.ChargifyId.Value, request.CancellationReason);

			if (!cancelled)
			{
				return new CancelSubscriptionResponse(ignoreCache: true)
				{
					Status = CancelSubscriptionStatus.CancellationFailed
				};
			}

            organisation.Subscription.ChargifyId = subscription.SubscriptionID;
            organisation.Subscription.Status = SubscriptionStatus.Cancelled;
            organisation.Subscription.CancellationDate = DateTime.UtcNow.ToDateTimeOffset(organisation.TimezoneId);
	        organisation.Subscription.CancellationReason = request.CancellationReason;

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

            return new CancelSubscriptionResponse(organisation.Id, request.CurrentUser.Id, request.CurrentUser.Email)
            {
                Status = CancelSubscriptionStatus.Ok,
				AccountExpirationDate = organisation.Subscription.CurrentPeriodEndsDate.Value
            };
        }
    }

    public interface ICancelSubscriptionCommand : ICommand<CancelSubscriptionRequest, CancelSubscriptionResponse>
    { }

    public class CancelSubscriptionResponse : CacheInvalidationResponseBase
    {
		public DateTimeOffset AccountExpirationDate { get; set; }
	    private readonly string _organisationId;
		private readonly string _userId;
		private readonly string _email;
        public CancelSubscriptionStatus Status { get; set; }

        public CancelSubscriptionResponse(string organisationId = null, string userId = null, string email = null, bool ignoreCache = false)
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

    public class CancelSubscriptionRequest : OrganisationRequestBase
    {
        public string CancellationReason { get; set; }
    }

    public enum CancelSubscriptionStatus
    {
        Ok,
        OrganisationNotFound,
        SubscriptionNotFound,
		CancellationFailed
    }
}
