
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using CodeTrip.Core.Misc;
using Errordite.Client;
using Errordite.Client.Configuration;
using Errordite.Client.Interfaces;
using CodeTrip.Core.Extensions;

namespace Errordite.Samples.Mvc3
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Product",
                            "product/{id}",
                            new {controller = "Product", action = "index"}
                );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        private class Logger : IErrorditeLogger
        {
            public void Debug(string message, params object[] args)
            {
                System.Diagnostics.Debug.WriteLine(message, args);
            }

            public void Error(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        protected void Application_Start()
        {
			log4net.Config.XmlConfigurator.Configure();

			System.Diagnostics.Trace.Write("Application_Start");

            AreaRegistration.RegisterAllAreas();

            ErrorditeClient.ConfigurationAugmenter = ErrorditeClientOverrideHelper.Augment;
            ErrorditeClient.SetLogger(new Logger());
			Errordite.Client.Log4net.ErrorditeLogger.Initialise(true, "Errordite.Samples");

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }


    }

}