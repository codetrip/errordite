using System;
using System.IO;
using System.Xml.Linq;
using Castle.Windsor;
using Errordite.Core.Extensions;

namespace Errordite.Core.IoC
{
    public class ConfigurationOverrideContainerInitialiser : ComponentBase, IContainerInitialiser
    {
        public void Init(IWindsorContainer container)
        {
            string configurationOverridePath = Environment.GetEnvironmentVariable("configurationoverridesfilepath");

            if (configurationOverridePath.IsNullOrEmpty())
                return;

            if (!File.Exists(configurationOverridePath))
                return;

            XDocument config = XDocument.Load(configurationOverridePath);

            foreach (var configObjectElement in config.Descendants("ConfigurationObject"))
            {
                string componentId = configObjectElement.SafeAttributeValue("ComponentId");

                if (componentId.IsNullOrEmpty())
                    continue;

                try
                {
                    ProcessConfigurationInstance(configObjectElement, container, componentId);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.Write(e.ToString());
                }
            }
        }

        private static void ProcessConfigurationInstance(XElement element, IWindsorContainer container, string componentId)
        {
            if (!container.Kernel.HasComponent(componentId))
                return;

            var component = container.Resolve(componentId, typeof(object));

            foreach (var configObjectElement in element.Descendants("Property"))
            {
                string propertyName = configObjectElement.SafeAttributeValue("Name");
                string propertyValue = configObjectElement.SafeAttributeValue("Value");

                component.SetPrimitiveToString(propertyName, propertyValue);
            }
        }
    }
}
