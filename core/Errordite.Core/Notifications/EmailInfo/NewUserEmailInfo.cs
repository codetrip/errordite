
namespace Errordite.Core.Notifications.EmailInfo
{
    public class NewUserEmailInfo : EmailInfoBase
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
