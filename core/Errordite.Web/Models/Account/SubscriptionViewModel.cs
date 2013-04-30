using System.Collections.Generic;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Models.Account
{
    public class SubscriptionViewModel
    {
        public IList<PaymentPlanViewModel> Plans { get; set; }
	    public Organisation Organisation { get; set; }
    }

    public class OrganisationSettingsViewModel
    {
        public string TimezoneId { get; set; }
        public string ApiKey { get; set; }
    }

    public class PaymentPlanViewModel
    {
        public PaymentPlan Plan { get; set; }
        public bool CurrentPlan { get; set; }
        public bool Upgrade { get; set; }
        public bool Downgrade { get; set; }
    }
}