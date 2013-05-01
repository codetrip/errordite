
namespace Errordite.Core.Notifications.EmailInfo
{
    public class SignUpCompleteEmailInfo : EmailInfoBase
    {
		public string UserName { get; set; }
		public string OrganisationName { get; set; }
		public string BillingAmount { get; set; }
        public string SubscriptionId { get; set; }
        public string PlanName { get; set; }
    }
}
