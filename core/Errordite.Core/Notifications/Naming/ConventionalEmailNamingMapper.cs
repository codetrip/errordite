using System;
using System.Reflection;

namespace Errordite.Core.Notifications.Naming
{
    public class ConventionalEmailNamingMapper : IEmailNamingMapper
    {
        public string InfoToTemplate(Type info)
        {
            return NameToTemplate(InfoToName(info));
        }

        public string InfoToName(Type info)
        {
            if (info.Name.EndsWith("Info", StringComparison.InvariantCultureIgnoreCase))
                return info.Name.Substring(0, info.Name.Length - 4);
            return info.Name;
        }

        public Type NameToInfo(string name)
        {
            return Assembly.GetExecutingAssembly().GetType("Errordite.Core.Notifications.EmailInfo." + name + "Info");
        }

        public string NameToTemplate(string name)
        {
            return name + "Template";
        }

        public string TemplateToName(string template)
        {
            if (template.EndsWith("Template", StringComparison.InvariantCultureIgnoreCase))
                return template.Substring(0, template.Length - 8);
            return template;
        }
    }
}