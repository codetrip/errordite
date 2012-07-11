using System.IO;
using CodeTrip.Core.FileSystem;
using Errordite.Core.Configuration;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Notifications.Exceptions;
using Errordite.Core.Notifications.Naming;

namespace Errordite.Core.Notifications.Rendering
{
    public class TemplateLocator : ITemplateLocator
    {
        private readonly EmailConfiguration _config;
        private readonly IEmailNamingMapper _emailNamingMapper;

        public TemplateLocator(EmailConfiguration config, IEmailNamingMapper emailNamingMapper)
        {
            _config = config;
            _emailNamingMapper = emailNamingMapper;
        }

        public string GetTemplate(EmailInfoBase emailInfo)
        {
            return GetTemplate(_emailNamingMapper.InfoToTemplate(emailInfo.GetType()));
        }

        public string GetTemplate(string templateName)
        {
            string resolvedPath = RepositoryPathHelper.ResolvePath(_config.TemplateLocation);
            string templateLocation = Path.Combine(resolvedPath, templateName + ".html");

            if (!File.Exists(templateLocation))
                throw new ErrorditeTemplateNotFoundException(templateLocation);

        	string template;

			using (var sr = new StreamReader(templateLocation))
				template = sr.ReadToEnd();

        	return template;
        }
    }
}