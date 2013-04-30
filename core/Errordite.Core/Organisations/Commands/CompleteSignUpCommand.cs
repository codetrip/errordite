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
    public class  CompleteSignUpCommand : SessionAccessBase, ICompleteSignUpCommand
    {
        private readonly IEncryptor _encryptor;
        private readonly ErrorditeConfiguration _configuration;

        public CompleteSignUpCommand(IEncryptor encryptor, ErrorditeConfiguration configuration)
        {
            _encryptor = encryptor;
            _configuration = configuration;
        }

        public CompleteSignUpResponse Invoke(CompleteSignUpRequest request)
        {
            Trace("Starting...");

            //verify  
            var connection = new ChargifyConnect(_configuration.ChargifyUrl, _configuration.ChargifyApiKey, _configuration.ChargifyPassword);
            var subscription = connection.LoadSubscription(request.SubscriptionId);

            if (subscription == null)
            {
                return new CompleteSignUpResponse(ignoreCache:true)
                {
                    Status = CompleteSignUpStatus.SubscriptionNotFound
                };
            }

            var token = _encryptor.Decrypt(HttpUtility.UrlDecode(request.Reference).Base64Decode()).Split('|');

            if (token[0] != request.CurrentUser.Organisation.FriendlyId)
            {
                return new CompleteSignUpResponse(ignoreCache: true)
                {
                    Status = CompleteSignUpStatus.InvalidOrganisation
                };
            }

            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.PaymentPlanId)
					.Load<Organisation>(request.CurrentUser.Organisation.Id);

            if (organisation == null)
            {
                return new CompleteSignUpResponse(ignoreCache: true)
                {
                    Status = CompleteSignUpStatus.OrganisationNotFound
                };
            }

			organisation.PaymentPlan = MasterLoad<PaymentPlan>(organisation.PaymentPlanId);
            organisation.Subscription.ChargifyId = subscription.SubscriptionID;
            organisation.Subscription.Status = SubscriptionStatus.Active;
            organisation.Subscription.StartDate = DateTime.UtcNow;
            organisation.Subscription.Dispensation = false;
            organisation.PaymentPlanId = "PaymentPlans/{0}".FormatWith(token[1]);

            Session.SynchroniseIndexes<Indexing.Organisations, Indexing.Users>();
			Session.AddCommitAction(new SendMessageCommitAction(
				new SignUpCompleteEmailInfo
				{
					OrganisationName = organisation.Name,
					SubscriptionId = request.SubscriptionId.ToString(),
					UserName = request.CurrentUser.FirstName,
					BillingAmount = string.Format(CultureInfo.GetCultureInfo(1033), "{0:C}", organisation.PaymentPlan.Price)
				},
				_configuration.GetNotificationsQueueAddress(organisation.RavenInstanceId)));

            return new CompleteSignUpResponse(organisation.Id, request.CurrentUser.Id, request.CurrentUser.Email)
            {
                Status = CompleteSignUpStatus.Ok
            };
        }
    }

    public interface ICompleteSignUpCommand : ICommand<CompleteSignUpRequest, CompleteSignUpResponse>
    { }

    public class CompleteSignUpResponse : CacheInvalidationResponseBase
    {
	    private readonly string _organisationId;
		private readonly string _userId;
		private readonly string _email;
        public CompleteSignUpStatus Status { get; set; }

        public CompleteSignUpResponse(string organisationId = null, string userId = null, string email = null, bool ignoreCache = false)
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

    public class CompleteSignUpRequest : OrganisationRequestBase
    {
        public int SubscriptionId { get; set; }
        public string Reference { get; set; }
    }

    public enum CompleteSignUpStatus
    {
        Ok,
        InvalidOrganisation,
        OrganisationNotFound,
        SubscriptionNotFound
    }
}
