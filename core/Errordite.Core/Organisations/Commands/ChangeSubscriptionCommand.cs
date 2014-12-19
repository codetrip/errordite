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
using Errordite.Core.Organisations.Queries;
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
        private readonly IGetOrganisationStatisticsQuery _getOrganisationStatisticsQuery;
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;

        public ChangeSubscriptionCommand(ErrorditeConfiguration configuration, 
            IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery, 
            IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery)
        {
            _configuration = configuration;
            _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
            _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
        }

        public ChangeSubscriptionResponse Invoke(ChangeSubscriptionRequest request)
        {
			Trace("Starting...");

			var organisation = Session.MasterRaven.Load<Organisation>(request.CurrentUser.ActiveOrganisation.Id);

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

            var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;
            var stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest()).Statistics;

            //if we are downgrading check the organisations stats to make sure they can
            if (request.Downgrading)
            {
				var plan = plans.FirstOrDefault(p => p.Id == PaymentPlan.GetId(request.NewPlanId));

                if (plan != null)
                {
                    var quotas = PlanQuotas.FromStats(stats, plan);

                    if (quotas.IssuesExceededBy > 0)
                    {
                        return new ChangeSubscriptionResponse(ignoreCache: true)
                        {
                            Status = ChangeSubscriptionStatus.QuotasExceeded,
                            Quotas = quotas
                        };
                    }
                }
            }

            var chargifyConnect = new ChargifyConnect(_configuration.ChargifyUrl, _configuration.ChargifyApiKey, _configuration.ChargifyPassword);
			var subscription = chargifyConnect.LoadSubscription(organisation.Subscription.ChargifyId.Value);

            if (subscription == null)
            {
                return new ChangeSubscriptionResponse(ignoreCache:true)
                {
                    Status = ChangeSubscriptionStatus.SubscriptionNotFound
                };
            }

            var newPlan = plans.First(p => p.Id == PaymentPlan.GetId(request.NewPlanId));

			if (newPlan.IsFreeTier)
			{
				chargifyConnect.DeleteSubscription(organisation.Subscription.ChargifyId.Value, "Downgrade");
				organisation.Subscription.ChargifyId = null;
				organisation.Subscription.Status = SubscriptionStatus.Trial;
			}
			else
			{
				subscription = chargifyConnect.EditSubscriptionProduct(organisation.Subscription.ChargifyId.Value, request.NewPlanName.ToLowerInvariant());
				organisation.Subscription.ChargifyId = subscription.SubscriptionID;
				organisation.Subscription.Status = SubscriptionStatus.Active;
			}

			organisation.PaymentPlanId = PaymentPlan.GetId(request.NewPlanId);
			organisation.PaymentPlan = newPlan;
			organisation.Subscription.LastModified = DateTime.UtcNow.ToDateTimeOffset(organisation.TimezoneId);

            //if status is quotas exceeded, check again and update if necessary
            if (organisation.Status == OrganisationStatus.PlanQuotaExceeded)
            {
                var quotas = PlanQuotas.FromStats(stats, newPlan);

                if (quotas.IssuesExceededBy <= 0)
                {
                    organisation.Status = OrganisationStatus.Active;
                }
            }

            Session.SynchroniseIndexes<Indexing.Organisations, Indexing.Users>();
			Session.AddCommitAction(new SendMessageCommitAction(
				new SubsciptionChangedEmailInfo
				{
					OrganisationName = organisation.Name,
					SubscriptionId = subscription.SubscriptionID.ToString(),
					UserName = request.CurrentUser.FirstName,
                    BillingAmount = string.Format(CultureInfo.GetCultureInfo(1033), "{0:C}", newPlan.Price),
					BillingPeriodEndDate = organisation.Subscription.CurrentPeriodEndDate.ToLocalFormatted(),
					OldPlanName = request.OldPlanName,
                    NewPlanName = newPlan.Name
				},
				_configuration.GetNotificationsQueueAddress(organisation.RavenInstanceId)));

            return new ChangeSubscriptionResponse(organisation.Id, request.CurrentUser.Email)
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
		private readonly string _email;
        public ChangeSubscriptionStatus Status { get; set; }
        public PlanQuotas Quotas { get; set; }

        public ChangeSubscriptionResponse(string organisationId = null, string email = null, bool ignoreCache = false)
            : base(ignoreCache)
        {
            _organisationId = organisationId;
            _email = email;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetOrganisationInvalidationItems(_organisationId, _email).Union(
                   CacheInvalidation.GetUserInvalidationItems(_organisationId, _email));
        }
    }

    public class ChangeSubscriptionRequest : OrganisationRequestBase
    {
        public string NewPlanId { get; set; }
		public string NewPlanName { get; set; }
        public string OldPlanName { get; set; }
        public bool Downgrading { get; set; }
    }

    public enum ChangeSubscriptionStatus
    {
        Ok,
        OrganisationNotFound,
        SubscriptionNotFound,
		QuotasExceeded
    }
}
