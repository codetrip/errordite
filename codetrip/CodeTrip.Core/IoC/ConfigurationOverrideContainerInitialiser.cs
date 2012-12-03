using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Castle.Windsor;
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.IoC
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
                    continue;
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

                if (propertyName.IsNullOrEmpty() || propertyValue.IsNullOrEmpty())
                    continue;

                var property = component.GetType().GetProperty(propertyName);

                if (TrySetProperty(property, component, propertyValue, int.Parse))
                    continue;
                if (TrySetProperty(property, component, propertyValue, long.Parse))
                    continue;
                if (TrySetProperty(property, component, propertyValue, decimal.Parse))
                    continue;
                if (TrySetProperty(property, component, propertyValue, DateTime.Parse))
                    continue;
                if (TrySetProperty(property, component, propertyValue, bool.Parse))                   
                    continue;
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(component, propertyValue, null);
                    continue;
                }
            }
        }



        private static bool TrySetProperty<T>(PropertyInfo property, object component, string overrideValue, Func<string, T> map) 
            where T : struct 
        {
            if (!property.PropertyType.IsIn(typeof(T), typeof(T?)))
                return false;

            if (overrideValue.IsNullOrEmpty() && property.PropertyType == typeof(T?))
                property.SetValue(component, null, null);
            else
                property.SetValue(component, map(overrideValue), null);

            return true;
        }
    }
}
