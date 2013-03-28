
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CodeTrip.Core.Misc;
using Errordite.Client;
using Errordite.Client.Configuration;
using Errordite.Client.Interfaces;

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

        protected void Application_Start()
        {
			log4net.Config.XmlConfigurator.Configure();

			System.Diagnostics.Trace.Write("Application_Start");

            AreaRegistration.RegisterAllAreas();

            ErrorditeClient.ConfigurationAugmenter = ErrorditeClientOverrideHelper.Augment;
            ErrorditeClient.ConfigurationAugmenter = c =>
                {
                    ErrorditeClientOverrideHelper.Augment(c);
                    c.DataCollectors.Insert(0, new AcmeDataCollectorFactory());
                };
            ErrorditeClient.SetErrorNotificationAction(e => System.Diagnostics.Trace.Write(e));
			Errordite.Client.Log4net.ErrorditeLogger.Initialise(true, "Errordite.Samples");

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }


    }

    public class AcmeDataCollectorFactory : IDataCollectorFactory, IDataCollector
    {
        public IDataCollector Create()
        {
            return this;
        }

        public string Prefix { get { return "ACME"; } }
        public ErrorData Collect(Exception e, IErrorditeConfiguration configuration)
        {
            var loginCookie = HttpContext.Current.Request.Cookies["login"];
            
            return new ErrorData()
                {
                    {"Username", loginCookie != null && loginCookie.Value != "" ? loginCookie.Value : "ANONYMOUS"}
                };
        }
    }


}