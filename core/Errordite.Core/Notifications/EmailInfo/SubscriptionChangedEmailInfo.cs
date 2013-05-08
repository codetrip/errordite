
namespace Errordite.Core.Notifications.EmailInfo
{
    public class SubsciptionChangedEmailInfo : EmailInfoBase
    {
		public string UserName { get; set; }
		public string OrganisationName { get; set; }
		public string BillingAmount { get; set; }
		public string SubscriptionId { get; set; }
		public string OldPlanName { get; set; }
		public string NewPlanName { get; set; }
		public string BillingPeriodEndDate { get; set; }
    }
}
