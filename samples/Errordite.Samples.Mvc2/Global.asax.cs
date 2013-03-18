using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Errordite.Client;

namespace Errordite.Samples.Mvc2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
            ErrorditeClient.ConfigurationAugmenter = ErrorditeClientOverrideHelper.Augment;
        }
    }

    //Can't reference CodeTrip.Core as it targets later framework version.
    public static class ErrorditeClientOverrideHelper
    {
        public static void Augment(object configuration)
        {
            string configurationOverridePath = Environment.GetEnvironmentVariable("configurationoverridesfilepath");

            if (string.IsNullOrEmpty(configurationOverridePath))
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
                        configuration.GetType().GetProperty(propertyOverride.Attribute("Name").Value).SetValue(configuration, propertyOverride.Attribute("Value").Value, null);
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