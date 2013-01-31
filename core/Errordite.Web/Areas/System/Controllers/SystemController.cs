using System;
using System.Collections.Generic;
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
using System.Linq;
using Raven.Client;

namespace Errordite.Web.Areas.System.Controllers
{
    [Authorize, RoleAuthorize]
    public class SystemController : AdminControllerBase
    {
        private readonly IAppSession _session;
        private readonly IDeleteApplicationCommand _deleteApplicationCommand;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IEncryptor _encryptor;
        private readonly IGetOrganisationsQuery _getOrganisationsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public SystemController(IAppSession session, 
            IDeleteApplicationCommand deleteApplicationCommand, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IEncryptor encryptor, 
            IGetOrganisationsQuery getOrganisationsQuery, 
            ErrorditeConfiguration configuration)
        {
            _session = session;
            _deleteApplicationCommand = deleteApplicationCommand;
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

        public ActionResult UpdateIssueCounts(string organisationId)
        {
            Core.Session.SetOrganisation(new Organisation
            {
                Id = Organisation.GetId(organisationId)
            });

            RavenQueryStatistics stats;

            var issues = Core.Session.Raven.Query<IssueDocument, Issues_Search>().Statistics(out stats)
                .Skip(0)
                .Take(_configuration.MaxPageSize)
                .As<Issue>()
                .ToList();

            if (stats.TotalResults > _configuration.MaxPageSize)
            {
                Trace("Total issues is greater than default page size, iterating to get all issues");
                int pageNumber = stats.TotalResults / _configuration.MaxPageSize;

                for (int i = 1; i < pageNumber; i++)
                {
                    issues.AddRange(Core.Session.Raven.Query<IssueDocument, Issues_Search>()
                        .Skip(pageNumber * _configuration.MaxPageSize)
                        .Take(_configuration.MaxPageSize)
                        .As<Issue>());
                }
            }

            foreach (var issue in issues)
            {
                Core.Session.Bus.Send(_configuration.EventsQueueName, new SyncIssueErrorCountsMessage
                {
                    IssueId = issue.Id,
                    OrganisationId = issue.OrganisationId
                });
            }

            return new EmptyResult();
        }

        public ActionResult SyncIssueCount(string organisationId, string issueId)
        {
            Core.Session.SetOrganisation(new Organisation
            {
                Id = Organisation.GetId(organisationId)
            });

            Core.Session.AddCommitAction(new SendNServiceBusMessage("Sync Issue Error Counts", new SyncIssueErrorCountsMessage
            {
                IssueId = Issue.GetId(issueId),
                OrganisationId = Organisation.GetId(organisationId)
            }, _configuration.EventsQueueName));

            return Content("Queued");
        }
    }
}
