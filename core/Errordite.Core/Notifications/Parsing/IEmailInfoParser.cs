using System.Collections.Generic;
using Errordite.Core.Notifications.EmailInfo;

namespace Errordite.Core.Notifications.Parsing
{
    public interface IEmailInfoParser
    {
        IDictionary<string, string> Parse(EmailInfoBase emailInfo);
    }
}