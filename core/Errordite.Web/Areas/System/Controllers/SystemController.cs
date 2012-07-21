using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.Models.Administration;
using Errordite.Web.Models.Navigation;
using Raven.Abstractions.Data;
using Raven.Abstractions.Linq;

namespace Errordite.Web.Areas.System.Controllers
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

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.SysAdmin)]
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
        public ActionResult CreatePaymentPlans()
        {
            if (!_session.Raven.Query<PaymentPlan>().Any())
            {
                _session.Raven.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/1",
                    MaximumApplications = 5,
                    MaximumUsers = 5,
                    MaximumIssues = 100,
                    PlanType = PaymentPlanType.Trial,
                    Price = 0m
                });
                _session.Raven.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/2",
                    MaximumApplications = 1,
                    MaximumUsers = 1,
                    MaximumIssues = 25,
                    PlanType = PaymentPlanType.Micro,
                    Price = 10.00m
                });
                _session.Raven.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/3",
                    MaximumApplications = 5,
                    MaximumUsers = 5,
                    MaximumIssues = 100,
                    PlanType = PaymentPlanType.Small,
                    Price = 35.00m
                });
                _session.Raven.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/4",
                    MaximumApplications = 30,
                    MaximumUsers = 30,
                    MaximumIssues = 250,
                    PlanType = PaymentPlanType.Big,
                    Price = 70.00m
                });
                _session.Raven.Store(new PaymentPlan
                {
                    Id = "PaymentPlans/5",
                    MaximumApplications = 100,
                    MaximumUsers = 100,
                    MaximumIssues = 1000,
                    PlanType = PaymentPlanType.Huge,
                    Price = 100.00m
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
