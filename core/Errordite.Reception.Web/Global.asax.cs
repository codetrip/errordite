using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Misc;
using Errordite.Client;
using Errordite.Client.Mvc;
using Errordite.Core.IoC;
using Errordite.Core.Session;
using Errordite.Reception.Web.Binders;
using Errordite.Reception.Web.IoC;
using NServiceBus;
using log4net.Config;

namespace Errordite.Reception.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorditeExceptionFilter());
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
            RegisterRoutes(RouteTable.Routes); ;

            //used to bind the ClientError Json posted by the client
            ModelBinders.Binders.Add(typeof(ClientError), new ClientErrorModelBinder());

            XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath(@"bin\config\log4net.config")));

            SetLoggingLibrary.Log4Net(XmlConfigurator.Configure);

            ObjectFactory.Container.Install(new ReceptionWebInstaller());

            ErrorditeClient.ConfigurationAugmenter = ErrorditeClientOverrideHelper.Augment;
            var controllerFactory = new WindsorControllerFactory(ObjectFactory.Container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                try
                {
                    ObjectFactory.GetObject<IComponentAuditor>().Error(GetType(), args.Exception);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.Write(e);
                }

                args.SetObserved();
            };
        }
    }
}