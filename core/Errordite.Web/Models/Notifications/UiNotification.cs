
using System.Web;
using System.Web.Mvc;

namespace Errordite.Web.Models.Notifications
{
    public class UiNotification
    {
        public MvcHtmlString Message { get; set; }
        public UiNotificationType Type { get; set; }

        public static UiNotification Error(MvcHtmlString message)
        {
            return new UiNotification
            {
                Message = message,
                Type = UiNotificationType.Error
            };
        }

        public static UiNotification Error(string unencodedMessage)
        {
            return Error(new MvcHtmlString(HttpUtility.HtmlEncode(unencodedMessage)));
        }

        public static UiNotification Confirmation(MvcHtmlString message)
        {
            return new UiNotification
            {
                Message = message,
                Type = UiNotificationType.Confirmation
            };
        }

        public static UiNotification Confirmation(string unencodedMessage)
        {
            return Confirmation(new MvcHtmlString(HttpUtility.HtmlEncode(unencodedMessage)));
        }

        public static UiNotification Info(MvcHtmlString message)
        {
            return new UiNotification
            {
                Message = message,
                Type = UiNotificationType.Info,
            };
        }

        public static UiNotification Info(string unencodedMessage)
        {
            return Info(new MvcHtmlString(HttpUtility.HtmlEncode(unencodedMessage)));
        }
    }

    public enum UiNotificationType
    {
        Error,
        Confirmation,
        Info
    }
}