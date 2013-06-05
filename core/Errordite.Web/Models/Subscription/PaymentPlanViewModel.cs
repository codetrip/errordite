
using System.Collections.Generic;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Models.Subscription
{
    public class SubscriptionSignUpViewModel
    {
        public IEnumerable<PaymentPlanViewModel> Plans { get; set; }
    }

	public class PaymentPlanViewModel
	{
		public string SignUpUrl { get; set; }
		public bool ViewFreeTier { get; set; }
		public PaymentPlan Plan { get; set; }
		public PaymentPlanStatus Status { get; set; }
	}

	public enum PaymentPlanStatus
	{
		FirstSignUp,
		SubscriptionSignUp,
		SelectedPlan
	}
}