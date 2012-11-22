using System;

namespace Errordite.Core.Notifications.Naming
{
    public interface IEmailNamingMapper
    {
        string InfoToTemplate(Type info);
        string InfoToName(Type info);
        string NameToTemplate(string name);
        string TemplateToName(string template);
        Type NameToInfo(string name);
    }
}