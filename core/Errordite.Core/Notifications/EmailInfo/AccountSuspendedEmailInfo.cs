
namespace Errordite.Core.Notifications.EmailInfo
{
    public class AccountSuspendedEmailInfo : EmailInfoBase
    {
        public string OrganisationName { get; set; }
        public string SuspendedReason { get; set; }
        public string UserName { get; set; }
    }
}
