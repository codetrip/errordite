
namespace Errordite.Web.Models.Subscription
{
	public class CancelSubscriptionViewModel : CancelSubscriptionPostModel
	{
		public Core.Domain.Organisation.Subscription Subscription { get; set; }
	}

	public class CancelSubscriptionPostModel
	{
		public string CancellationReason { get; set; }
	}
}