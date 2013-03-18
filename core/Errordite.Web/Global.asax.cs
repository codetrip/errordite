using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Core.Internal;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Misc;
using Errordite.Client;
using Errordite.Core;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.IoC;
using Errordite.Core.Session;
using Errordite.Core.WebApi;    
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.IoC;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Extensions;
using Raven.Client.Indexes;

namespace Errordite.Web
{
    public class ErrorditeApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
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
                name: "issueapi",
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

            //web API config
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorHttpControllerActivator());
            GlobalConfiguration.Configuration.DependencyResolver = new WindsorDependencyResolver(ObjectFactory.Container);
            //this has the effect of always defaulting to Json serialization as there are no Xml formatters registered
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = WebApiSettings.JsonSerializerSettings;

            ErrorditeClient.ConfigurationAugmenter = ErrorditeClientOverrideHelper.Augment;

#if !(DEBUG)
            BootstrapRaven(ObjectFactory.Container.Resolve<IDocumentStore>());
#endif
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

        #region Bootstrap Raven

        public static void BootstrapRaven(IDocumentStore documentStore)
        {
            documentStore.DatabaseCommands.EnsureDatabaseExists(CoreConstants.ErrorditeMasterDatabaseName);

            var session = documentStore.OpenSession(CoreConstants.ErrorditeMasterDatabaseName);

            IndexCreation.CreateIndexes(new CompositionContainer(
                new AssemblyCatalog(typeof(Issues_Search).Assembly), new ExportProvider[0]),
                documentStore.DatabaseCommands.ForDatabase(CoreConstants.ErrorditeMasterDatabaseName),
                documentStore.Conventions);

            if (!session.Query<PaymentPlan>().Any())
            {
                session.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/1",
                    MaximumApplications = 10,
                    MaximumUsers = 20,
                    MaximumIssues = 500,
                    Name = PaymentPlanNames.Trial,
                    Rank = 0,
                    Price = 0m,
                    IsAvailable = true,
                    IsTrial = true,
                });
                session.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/2",
                    MaximumApplications = 1,
                    MaximumUsers = 2,
                    MaximumIssues = 50,
                    Name = PaymentPlanNames.Small,
                    Rank = 100,
                    Price = 19.00m,
                    IsAvailable = true,
                });
                session.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/3",
                    MaximumApplications = 10,
                    MaximumUsers = 20,
                    MaximumIssues = 500,
                    Name = PaymentPlanNames.Medium,
                    Rank = 200,
                    Price = 79.00m,
                    IsAvailable = true,
                });
                session.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/4",
                    MaximumApplications = 100,
                    MaximumUsers = 200,
                    MaximumIssues = 5000,
                    Name = PaymentPlanNames.Large,
                    Rank = 300,
                    Price = 299.00m,
                    IsAvailable = true,
                });

                session.SaveChanges();
            }

            var organisations = session.Query<Organisation, Organisations_Search>();

            foreach (var organisation in organisations)
            {
                session.Advanced.DocumentStore.DatabaseCommands.EnsureDatabaseExists(organisation.FriendlyId);

                IndexCreation.CreateIndexes(
                    new CompositionContainer(new AssemblyCatalog(typeof(Issues_Search).Assembly), new ExportProvider[0]), 
                    session.Advanced.DocumentStore.DatabaseCommands.ForDatabase(organisation.FriendlyId), 
                    documentStore.Conventions);

                using (var organisationSession = documentStore.OpenSession(organisation.FriendlyId))
                {
                    var facets = new List<Facet>
                    {
                        new Facet {Name = "Status"},
                    };

                    organisationSession.Store(new FacetSetup { Id = CoreConstants.FacetDocuments.IssueStatus, Facets = facets });
                    organisationSession.SaveChanges();
                }
            }
        }

        #endregion
    }
}