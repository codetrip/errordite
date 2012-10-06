using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Core.Internal;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Web;
using Errordite.Client.Log4net;
using Errordite.Client.Mvc3;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Indexing;
using Errordite.Core.IoC;
using Errordite.Core.Session;
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.IoC;
using Raven.Client;
using Raven.Client.Indexes;

namespace Errordite.Web
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorditeExceptionFilter());
            filters.Add(new ViewDataActionFilter());
            filters.Add(new SessionActionFilterAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("profiler/{*pathInfo}");
            routes.IgnoreRoute("errorditelogging/{*pathInfo}");
            routes.MapRoute(
                "Home", // Route name
                "{action}", // URL with parameters
                new { controller = "Home", action = "Index" },
                new { action = "(contact|index)" }// Parameter defaults
            );

            routes.MapRoute(
                "IssueSpecial",
                "{controller}/{action}",
                null,
                new { controller = "issue", action = "getreportdata|errors|adjustrules|adjustdetails|purge|import|delete" });


            routes.MapRoute(
                "EntityWithId",
                "{controller}/{id}",
                new {action = "Index"},
                new {controller = "issue", action = "index"}
                );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath(@"bin\config\log4net.config")));

            ObjectFactory.Container.Install(new WebInstaller());

            var controllerFactory = new WindsorControllerFactory(ObjectFactory.Container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            var mappers = ObjectFactory.Container.ResolveAll(typeof(IMappingDefinition));

            if (mappers != null && mappers.Length > 0)
                mappers.Cast<IMappingDefinition>().ForEach(b => b.Define());

            var cultureInfo = new CultureInfo("en-GB")
            {
                DateTimeFormat =
                {
                    ShortDatePattern = "dd/MM/yyyy", 
                    DateSeparator = "/",
                }
            };

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            IndexCreation.CreateIndexes(typeof(Issues_Search).Assembly, ObjectFactory.GetObject<IDocumentStore>());

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

            ErrorditeLogger.Initialise(true, "Errordite.Web");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (Context.IsCustomErrorEnabled)
                ShowCustomErrorPage(Server.GetLastError());
        }

        /// <summary>
        /// This handles custom error pages, the asp.net customErrors config does not suffice as it returns incorrect status codes (always 200)
        /// Logging is not handled by this method, our ErrorditeExceptionFilter will kick in before this and log error to Errordite, log4net error reporting
        /// isssss not enabled for the web app.
        /// </summary>
        /// <param name="exception"></param>
        private void ShowCustomErrorPage(Exception exception)
        {
            HttpException httpException = exception as HttpException ?? new HttpException(500, "Internal Server Error", exception);

            Response.Clear();
            Response.StatusCode = httpException.GetHttpCode();

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("fromAppErrorEvent", true);

            switch (httpException.GetHttpCode())
            {
                case 403:
                    routeData.Values.Add("action", "NotAuthorised");
                    break;
                case 404:
                    routeData.Values.Add("action", "NotFound");
                    break;
                case 500:
                    if (exception is ErrorditeAuthorisationException)
                    {
                        Response.StatusCode = 403;
                        routeData.Values.Add("action", "NotAuthorised");
                    }
                    else
                    {
                        Response.StatusCode = 500;
                        routeData.Values.Add("action", "ServerError");
                    }
                       
                    break;
                default:
                    routeData.Values.Add("action", "OtherHttpStatusCode");
                    routeData.Values.Add("httpStatusCode", httpException.GetHttpCode());
                    break;
            }

            Server.ClearError();
            IController controller = new ErrorController();
            controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }
    }
}