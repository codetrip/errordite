using System;
using System.Collections.Generic;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Models.Subscription
{
    public class SubscriptionViewModel
    {
        public IList<ChangePaymentPlanViewModel> Plans { get; set; }
	    public Organisation Organisation { get; set; }
    }

    public class OrganisationSettingsViewModel
    {
        public string TimezoneId { get; set; }
        public string ApiKey { get; set; }
    }

    public class ChangePaymentPlanViewModel
    {
        public PaymentPlan Plan { get; set; }
        public bool CurrentPlan { get; set; }
        public bool Upgrade { get; set; }
        public bool Downgrade { get; set; }
        public bool SignUp { get; set; }
    }

    public class ChangeSubscriptionViewModel : ChangeSubscriptionPostModel
    {
        public PaymentPlan CurrentPlan { get; set; }
        public PaymentPlan NewPlan { get; set; }
        public DateTimeOffset CurrentBillingPeriodEnd { get; set; }
    }

    public class ChangeSubscriptionPostModel
    {
        public string NewPlanId { get; set; }
		public string NewPlanName { get; set; }
        public string OldPlanName { get; set; }
        public bool Downgrading { get; set; }
    }
}