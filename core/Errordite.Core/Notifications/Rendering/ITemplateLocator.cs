using Errordite.Core.Notifications.EmailInfo;

namespace Errordite.Core.Notifications.Rendering
{
    public interface ITemplateLocator
    {
        string GetTemplate(EmailInfoBase emailInfo);
        string GetTemplate(string templateName);
    }
    
}