using System;
using System.IO;
using System.Xml.Linq;
using Errordite.Core.Extensions;

namespace Errordite.Core.Misc
{
    /// <summary>
    /// Bit of a horrible hack this in that we expect the configuration to be of type IErrorditeConfiguration.
    /// We want this code to be shared between different apps but don't want to reference any client code so
    /// we'll just use object - the properties are set by reflection so it is not a problem to do this.
    /// </summary>
    public static class ErrorditeClientOverrideHelper
    {
        public static void Augment(object configuration)
        {
            string configurationOverridePath = Environment.GetEnvironmentVariable("configurationoverridesfilepath");

            if (configurationOverridePath.IsNullOrEmpty())
                return;

            if (!File.Exists(configurationOverridePath))
                return;

            XDocument config = XDocument.Load(configurationOverridePath);

            foreach (var clientOverride in config.Descendants("ErrorditeClient"))
            {
                try
                {
                    foreach (var propertyOverride in clientOverride.Descendants("Property"))
                    {
                        configuration.SetPrimitiveToString(propertyOverride.Attribute("Name").Value, propertyOverride.Attribute("Value").Value);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.Write(e.ToString());
                    continue;
                }
            }
        }
    }
}