using System.Collections.Generic;
using Errordite.Core.Notifications.Sending;

namespace Errordite.Core.Notifications.Rendering
{
    public interface IEmailRenderer
    {
        Message Render(string template, IDictionary<string, string> emailParams);
    }
}