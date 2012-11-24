
namespace Errordite.Core.Notifications.EmailInfo
{
    public class ResetPasswordEmailInfo : EmailInfoBase
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
