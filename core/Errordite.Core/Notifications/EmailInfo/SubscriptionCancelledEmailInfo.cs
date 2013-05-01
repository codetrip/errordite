
namespace Errordite.Core.Notifications.EmailInfo
{
    public class SubscriptionCancelledEmailInfo : EmailInfoBase
    {
		public string UserName { get; set; }
		public string OrganisationName { get; set; }
		public string AccountDisabledOn { get; set; }
    }
}
