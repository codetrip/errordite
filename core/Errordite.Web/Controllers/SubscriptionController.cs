using System.Linq;
using System.Web;
using System.Web.Mvc;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Encryption;
using Errordite.Core.Organisations.Queries;
using Errordite.Web.Models.Subscription;
using Errordite.Core.Extensions;
using ChargifyNET;

namespace Errordite.Web.Controllers
{
	[Authorize]
    public class SubscriptionController : ErrorditeController
    {
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;
		private readonly IEncryptor _encryptor;
		private readonly ErrorditeConfiguration _configuration;

        public SubscriptionController(IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery, 
			IEncryptor encryptor, 
			ErrorditeConfiguration configuration)
        {
	        _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
	        _encryptor = encryptor;
	        _configuration = configuration;
        }

		[HttpGet]
        public ActionResult TrialExpired()
        {
            var paymentPlans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans.Where(p => !p.IsTrial);

            return View(paymentPlans.Select(p => new PaymentPlanViewModel
	        {
		        Plan = p,
				Status = PaymentPlanStatus.SubscriptionSignUp,
				SignUpUrl = "{0}{1}".FormatWith(p.SignUpUrl, GetSignUpToken(p.FriendlyId))
	        }));
        }

		public ActionResult Complete(SubscriptionCompleteViewModel model)
		{
			//verify  
			var connection = new ChargifyConnect(_configuration.ChargifyUrl, _configuration.ChargifyApiKey, _configuration.ChargifyPassword);
			var subscription = connection.LoadSubscription(model.SubscriptionId);
			var token = _encryptor.Decrypt(HttpUtility.UrlDecode(model.Reference).Base64Decode()).Split('|');

			if (token[0] != Core.AppContext.CurrentUser.Organisation.FriendlyId)
			{
				model.Status = SignUpStatus.InvalidOrganisation;
				return View(model);
			}

			var organisation = Core.Session.MasterRaven.Load<Organisation>(Core.AppContext.CurrentUser.Organisation.Id);

			if (organisation == null)
			{
				model.Status = SignUpStatus.InvalidOrganisation;
				return View(model);
			}

			organisation.SubscriptionId = subscription.SubscriptionID;
			organisation.SubscriptionStatus = SubscriptionStatus.Active;
			organisation.PaymentPlanId = "PaymentPlans/{0}".FormatWith(token[1]);

			model.Status = SignUpStatus.Ok;
			return View(model);
		}

		private string GetSignUpToken(string planId)
		{
			return "?first_name={0}&last_name={1}&email={2}&organisation={3}&reference={4}".FormatWith(
				Core.AppContext.CurrentUser.FirstName,
				Core.AppContext.CurrentUser.LastName,
				Core.AppContext.CurrentUser.Email,
				Core.AppContext.CurrentUser.Organisation.Name,
				HttpUtility.UrlEncode(_encryptor.Encrypt("{0}|{1}".FormatWith(
					Core.AppContext.CurrentUser.Organisation.FriendlyId,
					planId)).Base64Encode()));
		}
    }
}
