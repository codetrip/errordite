
namespace Errordite.Web.Models.Subscription
{
	public class SubscriptionCompleteViewModel
	{
		public int SubscriptionId { get; set; }
		public string Reference { get; set; }
		public SignUpStatus Status { get; set; }
	}

	public enum SignUpStatus
	{
		Ok,
		InvalidOrganisation,
		OrganisationNotFound,
	}
}