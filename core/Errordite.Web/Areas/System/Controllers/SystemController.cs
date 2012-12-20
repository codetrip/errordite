using System;
using System.Web.Mvc;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Messages;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Web.ActionFilters;
using Errordite.Web.Areas.System.Models.System;
using Errordite.Web.Controllers;
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
        private readonly IGetOrganisationsQuery _getOrganisationsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public SystemController(IAppSession session, 
            IDeleteApplicationCommand deleteApplicationCommand, 
            IGetErrorditeErrorsQuery getErrorditeErrorsQuery, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IEncryptor encryptor, 
            IGetOrganisationsQuery getOrganisationsQuery, 
            ErrorditeConfiguration configuration)
        {
            _session = session;
            _deleteApplicationCommand = deleteApplicationCommand;
            _getErrorditeErrorsQuery = getErrorditeErrorsQuery;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _encryptor = encryptor;
            _getOrganisationsQuery = getOrganisationsQuery;
            _configuration = configuration;
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
            _session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.ErrorditeErrors, new IndexQuery
            {
                Query = "TimestampUtc:[* TO {0}]".FormatWith(DateTools.DateToString(fromDate, DateTools.Resolution.MILLISECOND))
            });

            ConfirmationNotification("Errordite Errors Purged Successfully.");

            return RedirectToAction("index");
        }

		[HttpPost, ExportViewData]
		public ActionResult SetupHourlyCounts(DateTime fromDate)
		{
			var organisations = _getOrganisationsQuery.Invoke(new GetOrganisationsRequest
			{
				Paging = new PageRequestWithSort(1, int.MaxValue)
			}).Organisations;

			foreach (var organisation in organisations.Items)
			{
				Core.Session.BootstrapOrganisation(organisation);

				foreach(var issue in Core.Session.Raven.Query<Issue>())
				{
					var issueHourlyCount = new IssueHourlyCount
					{
						IssueId = issue.Id,
						Id = "IssueHourlyCount/{0}".FormatWith(issue.FriendlyId)
					};

					issueHourlyCount.Initialise();
					Core.Session.Raven.Store(issueHourlyCount);
				}
			}

			return RedirectToAction("index");
		}

        [HttpPost, ExportViewData]
        public ActionResult RebuildIndex(string indexName)
        {
			_session.RavenDatabaseCommands.ResetIndex(indexName);
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

        public ActionResult Decrypt(string token, bool base64)
        {
            if (base64)
                token = token.Base64Decode();

            return Content(_encryptor.Decrypt(token));
        }

        public ActionResult Encrypt(string token, bool base64)
        {
            var encrypted = _encryptor.Encrypt(token);

            if (base64)
                encrypted = encrypted.Base64Encode();

            return Content(encrypted);
        }

        public ActionResult SysInfo()
        {
            return Content(Environment.ProcessorCount.ToString());
        }

        public ActionResult CreateIndexes(int organisationId)
        {
            Core.Session.BootstrapOrganisation(Core.Session.MasterRaven.Load<Organisation>(Organisation.GetId(organisationId.ToString())));

            return new EmptyResult();
        }

        public ActionResult DoError()
        {
            Trace("This is a test logging message");
            throw new InvalidOperationException("Something went wrong");
        }

        public ActionResult SyncIndexes()
        {
            var organisations = _getOrganisationsQuery.Invoke(new GetOrganisationsRequest
            {
                Paging = new PageRequestWithSort(1, int.MaxValue)
            }).Organisations;

            foreach (var organisation in organisations.Items)
            {
                Core.Session.BootstrapOrganisation(organisation);
            }

            return new EmptyResult();
        }

        public ActionResult UpdateIssueCounts()
        {
            var organisations = _getOrganisationsQuery.Invoke(new GetOrganisationsRequest
            {
                Paging = new PageRequestWithSort(1, int.MaxValue)
            }).Organisations;

            foreach (var organisation in organisations.Items)
            {
                Core.Session.SetOrganisation(organisation);

                foreach (var issue in Core.Session.Raven.Query<Issue, Issues_Search>())
                {
                    Core.Session.AddCommitAction(new SendNServiceBusMessage("Sync Issue Error Counts", new SyncIssueErrorCountsMessage
                    {
                        IssueId = issue.Id,
                        OrganisationId = issue.OrganisationId
                    }, _configuration.EventsQueueName));
                }
            }

            return new EmptyResult();
        }
    }
}
