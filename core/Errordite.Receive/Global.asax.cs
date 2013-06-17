﻿using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Errordite.Core.IoC;
using Errordite.Client;
using Errordite.Core.Session;
using Errordite.Core.Web.Binders;
using Errordite.Receive.IoC;
using log4net.Config;

namespace Errordite.Receive
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SessionActionFilterAttribute());
        }

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

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //used to bind the ClientError Json posted by the client
            ModelBinders.Binders.Add(typeof(ClientError), new ClientErrorModelBinder());

            XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath(@"bin\config\log4net.config")));

            ObjectFactory.Container.Install(new ReceiveInstaller());

            var controllerFactory = new WindsorControllerFactory(ObjectFactory.Container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
        }
    }
}