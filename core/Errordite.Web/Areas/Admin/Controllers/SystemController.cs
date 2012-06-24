using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.Models.Administration;
using Errordite.Web.Models.Navigation;
using Raven.Abstractions.Data;
using Raven.Abstractions.Linq;

namespace Errordite.Web.Areas.Admin.Controllers
{
    [Authorize, RoleAuthorize]
    public class SystemController : ErrorditeController
    {
        private readonly IAppSession _session;
        private readonly IDeleteApplicationCommand _deleteApplicationCommand;
        private readonly IGetErrorditeErrorsQuery _getErrorditeErrorsQuery;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IEncryptor _encryptor;

        public SystemController(IAppSession session, 
            IDeleteApplicationCommand deleteApplicationCommand, 
            IGetErrorditeErrorsQuery getErrorditeErrorsQuery, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IEncryptor encryptor)
        {
            _session = session;
            _deleteApplicationCommand = deleteApplicationCommand;
            _getErrorditeErrorsQuery = getErrorditeErrorsQuery;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _encryptor = encryptor;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Admin)]
        public ActionResult Index()
        {
            return View();
        }

        [PagingView, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.AdminErrors)]
        public ActionResult ErrorditeErrors(ErrorditeErrorsPostModel postModel)
        {
            var viewModel = new ErrorditeErrorsViewModel();
            var pagingRequest = GetSinglePagingRequest();

            var request = new GetErrorditeErrorsRequest
            {
                Paging = pagingRequest,
                Query = postModel.Query,
                Application = postModel.Application,
                ExceptionType = postModel.ExceptionType,
                StartDate = postModel.StartDate,
                EndDate = postModel.EndDate,
                MessageId = postModel.MessageId
            };

            var errors = _getErrorditeErrorsQuery.Invoke(request);

            viewModel.ExceptionType = postModel.ExceptionType;
            viewModel.Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, errors.Errors.PagingStatus, pagingRequest);
            viewModel.Errors = errors.Errors;
            viewModel.Application = postModel.Application;
            viewModel.StartDate = postModel.StartDate;
            viewModel.EndDate = postModel.EndDate;

            return View(viewModel);
        }

        [HttpPost, ExportViewData]
        public ActionResult DeleteApplicationErrors(string applicationId)
        {
            _deleteApplicationCommand.Invoke(new DeleteApplicationRequest
            {
                ApplicationId = applicationId,
                JustDeleteErrors = true,
                CurrentUser = null
            });

            ConfirmationNotification("Applications errors and classes have been deleted successfully.");

            return RedirectToAction("index");
        }

        [HttpPost, ExportViewData]
        public ActionResult PurgeErrorditeErrors(DateTime fromDate)
        {
            _session.Raven.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.ErrorditeErrors, new IndexQuery
            {
                Query = "TimestampUtc:[* TO {0}]".FormatWith(DateTools.DateToString(fromDate, DateTools.Resolution.MILLISECOND))
            });

            ConfirmationNotification("Errordite Errors Purged Successfully.");

            return RedirectToAction("index");
        }

        [ExportViewData]
        public ActionResult CreateNotifications()
        {
            foreach (var name in Enum.GetNames(typeof(NotificationType)))
            {
                _session.Raven.Store(new Notification { Type = (NotificationType)Enum.Parse(typeof(NotificationType), name) });
            }

            ConfirmationNotification("Successfully seeded site data");
            return RedirectToAction("index");
        }

        [ExportViewData]
        public ActionResult CreatePaymentPlans()
        {
            if (!_session.Raven.Query<PaymentPlan>().Any())
            {
                _session.Raven.Store(new PaymentPlan
                {
                    MaximumApplications = 1,
                    MaximumUsers = 5,
                    PlanType = PaymentPlanType.Trial,
                    Price = 0m
                });
                _session.Raven.Store(new PaymentPlan
                {
                    MaximumApplications = 1,
                    MaximumUsers = 5,
                    PlanType = PaymentPlanType.Small,
                    Price = 25.00m
                });
                _session.Raven.Store(new PaymentPlan
                {
                    MaximumApplications = 5,
                    MaximumUsers = 10,
                    PlanType = PaymentPlanType.Medium,
                    Price = 35.00m
                });
                _session.Raven.Store(new PaymentPlan
                {
                    MaximumApplications = 50,
                    MaximumUsers = 100,
                    PlanType = PaymentPlanType.Large,
                    Price = 50.00m
                });
            }

            ConfirmationNotification("Successfully created payment plans");
            return RedirectToAction("index");
        }

        [HttpPost, ExportViewData]
        public ActionResult RebuildIndex(string indexName)
        {
            _session.Raven.Advanced.DocumentStore.DatabaseCommands.ResetIndex(indexName);
            ConfirmationNotification("Index '{0}' was successfully rebuilt.");
            return RedirectToAction("index");
        }

        public ActionResult GetToken(string applicationId)
        {
            var application = _session.Raven.Load<Application>(Application.GetId(applicationId));

            return new ContentResult
            {
                Content = _encryptor.Encrypt("{0}|{1}".FormatWith(application.FriendlyId, application.OrganisationId.GetFriendlyId()))
            };
        }

        public ActionResult SysInfo()
        {
            return Content(Environment.ProcessorCount.ToString());
        }

        public ActionResult DoError()
        {
            Trace("This is a test logging message");
            throw new InvalidOperationException("Something went wrong");
        }

        public ActionResult UpdateOrganisations()
        {
            var organisations = _session.Raven.Query<Organisation, Organisations_Search>().ToList();

            foreach (var organisation in organisations)
            {
                organisation.TimezoneId = "UTC";
                organisation.CreatedOnUtc = DateTime.UtcNow.AddMonths(-6);
            }

            return new ContentResult
            {
                Content = "Done"
            };
        }

        public void SeedFacets()
        {
            var _facets = new List<Facet>
            {
                new Facet {Name = "Status"},
            };

            _session.Raven.Store(new FacetSetup { Id = CoreConstants.FacetDocuments.IssueStatus, Facets = _facets });
        }
    }
}
