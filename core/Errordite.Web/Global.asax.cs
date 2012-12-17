using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
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
using CodeTrip.Core.Paging;
using Errordite.Client.Mvc3;
using Errordite.Core;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.IoC;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.IoC;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Extensions;
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

           // ErrorditeLogger.Initialise(true, "Errordite.Web");
            BootstrapRaven();
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

        private void BootstrapRaven()
        {
            var documentStore = ObjectFactory.GetObject<IDocumentStore>();
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
                    MaximumApplications = 5,
                    MaximumUsers = 5,
                    MaximumIssues = 100,
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
                    MaximumUsers = 1,
                    MaximumIssues = 25,
                    Name = PaymentPlanNames.Micro,
                    Rank = 100,
                    Price = 10.00m,
                    IsAvailable = true,
                });
                session.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/3",
                    MaximumApplications = 5,
                    MaximumUsers = 5,
                    MaximumIssues = 100,
                    Name = PaymentPlanNames.Small,
                    Rank = 200,
                    Price = 35.00m,
                    IsAvailable = true,
                });
                session.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/4",
                    MaximumApplications = 30,
                    MaximumUsers = 30,
                    MaximumIssues = 250,
                    Name = PaymentPlanNames.Big,
                    Rank = 300,
                    Price = 70.00m,
                    IsAvailable = true,
                });
                session.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/5",
                    MaximumApplications = 100,
                    MaximumUsers = 100,
                    MaximumIssues = 1000,
                    Name = PaymentPlanNames.Huge,
                    Rank = 400,
                    Price = 100.00m,
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