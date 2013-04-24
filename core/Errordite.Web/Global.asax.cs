using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Core.Internal;
using Errordite.Core.Domain.Master;
using Errordite.Core.Interfaces;
using Errordite.Core.IoC;
using Errordite.Core.Misc;
using Errordite.Client;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.IoC;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Raven;
using Errordite.Core.Session;
using Errordite.Core.Web;
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.IoC;

namespace Errordite.Web
{
    public class ErrorditeApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new SelectApplicationActionFilter());
            filters.Add(new ViewDataActionFilter());
            filters.Add(new SessionActionFilterAttribute());
        }

        public static void RegisterWebApiFilters(System.Web.Http.Filters.HttpFilterCollection filters)
        {
            filters.Add(new SessionActionFilter());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("profiler/{*pathInfo}");
            routes.IgnoreRoute("errorditelogging/{*pathInfo}");

            routes.MapHttpRoute(
                name: "issuesapi",
                routeTemplate: "api/issues/{id}",
                defaults: new { controller = "issueapi", id = RouteParameter.Optional }
            );
            routes.MapHttpRoute(
                name: "applicationapi",
                routeTemplate: "api/applications/{id}",
                defaults: new { controller = "applicationapi", id = RouteParameter.Optional }
            );

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
                new { controller = "issue", action = "getreportdata|errors|adjustrules|purge|reprocess|delete|addcomment|history|whatifreprocess" });


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
            RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters);
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

            //web API config
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorHttpControllerActivator());
            GlobalConfiguration.Configuration.DependencyResolver = new WindsorDependencyResolver(ObjectFactory.Container);
            //this has the effect of always defaulting to Json serialization as there are no Xml formatters registered
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = WebApiSettings.JsonSerializerSettings;

            ErrorditeClient.ConfigurationAugmenter = ErrorditeClientOverrideHelper.Augment;

            BootstrapRavenInstances();
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

            var routeData = new RouteData();
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
            IController controller = new ErrorController(null);
            controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }

        public static void BootstrapRavenInstances()
        {
            var masterDocumentStoreFactory = ObjectFactory.GetObject<IShardedRavenDocumentStoreFactory>();
            var masterDocumentStore = masterDocumentStoreFactory.Create(RavenInstance.Master());
            var session = masterDocumentStore.OpenSession(CoreConstants.ErrorditeMasterDatabaseName);
            var instances = ObjectFactory.GetObject<IGetRavenInstancesQuery>().Invoke(new GetRavenInstancesRequest
                {
                    Session = session
                }).RavenInstances;

            var master = instances.FirstOrDefault(i => i.IsMaster);

            if(master != null)
            {
                master.ReceiveQueueAddress = ErrorditeConfiguration.Current.GetReceiveQueueAddress(string.Empty);
                master.NotificationsQueueAddress = ErrorditeConfiguration.Current.GetNotificationsQueueAddress();
                master.EventsQueueAddress = ErrorditeConfiguration.Current.GetEventsQueueAddress();
                session.SaveChanges();
            }
        }
    }
}