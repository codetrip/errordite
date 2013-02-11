using System;
using System.Collections.Generic;
using Errordite.Core.Configuration;
using Errordite.Core.ServiceBus;

namespace Errordite.Core.Notifications.EmailInfo
{
    public interface IAlertInfo
    {
        IEnumerable<string> ToUserIds { get; set; }
        string[] Replacements { get; }
        string MessageTemplate { get; }
    }

    /// <summary>
    /// Base class for sending an email.
    /// </summary>
    public abstract class EmailInfoBase : ErrorditeNServiceBusMessageBase
    {
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }

        public string CurrentYear
        {
            get { return DateTime.UtcNow.Year.ToString(); }
        }

        public virtual string ConvertToSimpleMessage(ErrorditeConfiguration configuration)
        {
            return string.Empty;
        }
    }

    public class NonTemplatedEmailInfo : EmailInfoBase
    {
        public string Body { get; set; }
        public string Subject { get; set; }
    }

    /// <summary>
    /// Interface to be implemented if there should be a custom ReplyTo address.  Note: do not implement implicitly as it uses reflection to pull it out.
    /// </summary>
    public interface ICustomReplyToEmail
    {
        string ReplyTo { get; }
    }
}
