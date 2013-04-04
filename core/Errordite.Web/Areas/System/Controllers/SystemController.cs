using System;
using System.ComponentModel.Composition.Hosting;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Raven;
using Errordite.Core.Session;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;
using Errordite.Core.Extensions;
using Raven.Client.Extensions;
using Raven.Client.Indexes;

namespace Errordite.Web.Areas.System.Controllers
{
    [Authorize, RoleAuthorize]
    public class SystemController : AdminControllerBase
    {
        private readonly IAppSession _session;
        private readonly IEncryptor _encryptor;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IShardedRavenDocumentStoreFactory _storeFactory;

        public SystemController(IAppSession session, 
            IEncryptor encryptor, 
            ErrorditeConfiguration configuration, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery,
            IShardedRavenDocumentStoreFactory storeFactory)
        {
            _session = session;
            _encryptor = encryptor;
            _configuration = configuration;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _storeFactory = storeFactory;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.SysAdmin)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, ExportViewData]
        public ActionResult SyncIndexes()
        {
            Server.ScriptTimeout = 7200; //timeout in 2 hours

            var masterDocumentStore = _storeFactory.Create(RavenInstance.Master());

            IndexCreation.CreateIndexes(new CompositionContainer(
                new AssemblyCatalog(typeof(Issues_Search).Assembly), new ExportProvider[0]),
                masterDocumentStore.DatabaseCommands.ForDatabase(CoreConstants.ErrorditeMasterDatabaseName),
                masterDocumentStore.Conventions);

            foreach (var organisation in Core.Session.MasterRaven.Query<Organisation>().GetAllItemsAsList(Core.Session, 100))
            {
                organisation.RavenInstance = Core.Session.MasterRaven.Load<RavenInstance>(organisation.RavenInstanceId);

                using (_session.SwitchOrg(organisation))
                {
                    _session.BootstrapOrganisation(organisation);
                }
            }

            ConfirmationNotification("All indexes for all organisations have been updated");
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

        public ActionResult SetApiKeys()
        {
            foreach (var organisation in Core.Session.MasterRaven.GetPage<Organisation, Organisations_Search, string>(new PageRequestWithSort(1, 128)).Items)
            {
                organisation.ApiKeySalt = Membership.GeneratePassword(8, 1);
                organisation.ApiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(_encryptor.Encrypt("{0}|{1}".FormatWith(organisation.FriendlyId, organisation.ApiKeySalt))));
            }

            return new EmptyResult();
        }

        public ActionResult UpdateTimezone()
        {
            var organisations = Core.Session.MasterRaven.Query<Organisation>();

            foreach (var org in organisations)
            {
                Core.Session.SetOrganisation(org, true);

                var apps = Core.Session.Raven.Query<Application, Applications_Search>().GetAllItemsAsList(Core.Session, _configuration.MaxPageSize);

                foreach (var app in apps)
                {
                    app.TimezoneId = org.TimezoneId;
                }
            }

            return new EmptyResult();
        }

        public ActionResult StripCss(string organisationId, string issueId)
        {
            Core.Session.SetOrganisation(new Organisation
            {
                Id = Organisation.GetId(organisationId)
            });

            var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
                {
                    IssueId = issueId,
                    OrganisationId = organisationId,
                    Paging = new PageRequestWithSort(1, 128)
                }).Errors.Items;

            foreach (var e in errors)
            {
                foreach (var info in e.ExceptionInfos)
                {
                    info.Message = info.Message.StripCss();
                    info.StackTrace = info.StackTrace.StripCss();
                }
            }

            Core.Session.Commit();

            return Content("Queued");
        }
    }
}
