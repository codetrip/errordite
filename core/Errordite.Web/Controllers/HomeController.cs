using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Web.Mvc;
using Errordite.Core;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.Caching.Resources;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Master;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.IoC;
using Errordite.Core.Configuration;
using Errordite.Core.Identity;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Raven;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Home;
using Errordite.Core.Extensions;
using Errordite.Web.Extensions;
using Raven.Client;
using Raven.Client.Indexes;

namespace Errordite.Web.Controllers
{
    public class HomeController : ErrorditeController
    {
		private readonly ErrorditeConfiguration _configuration;
		private readonly IAppSession _session;
		private readonly IShardedRavenDocumentStoreFactory _storeFactory;
		private int _errorCount = 0;
		private int _errorIgnoredCount = 0;
		private int _errorDeletedCount = 0;

        public HomeController(ErrorditeConfiguration configuration, 
			IShardedRavenDocumentStoreFactory storeFactory, 
			IAppSession session)
        {
	        _configuration = configuration;
	        _storeFactory = storeFactory;
	        _session = session;
        }

		[HttpGet, ImportViewData]
		public ActionResult Errors()
		{
			Server.ScriptTimeout = 7200; //timeout in 2 hours
			_errorCount = 0;
			_errorIgnoredCount = 0;
			_errorDeletedCount = 0;
			var session = ObjectFactory.GetObject<IAppSession>();

			var ravenInstances = session.MasterRaven.Query<RavenInstance>().ToList();

			foreach (var organisation in session.MasterRaven.Query<Organisation, Organisations>())
			{
				organisation.RavenInstance = ravenInstances.First(r => r.Id == organisation.RavenInstanceId);

				using (_session.SwitchOrg(organisation))
				{
					RavenQueryStatistics stats;

					foreach (var error in _session.Raven.Query<ErrorDocument, Errors>().Statistics(out stats)
						.Skip(0)
						.Take(25)
						.As<Error>()
                        .OrderBy(e => e.FriendlyId)
						.ToList())
					{
						if (_session.Raven.Load<Issue>(error.IssueId) == null)
						{
							_errorDeletedCount++;
							_session.Raven.Delete(error);
						}
						else
						{
							ProcessError(error);
						}
					}

					_session.Commit();
                    _session.Close();
                    new SynchroniseIndexCommitAction<Errors>().Execute(_session);
					Trace("Committed page 1, processed {0}, ignored {1}, deleted {2}", _errorCount, _errorIgnoredCount, _errorDeletedCount);

					if (stats.TotalResults > 25)
					{
						int pageNumber = stats.TotalResults / 25;

						for (int i = 1; i < pageNumber; i++)
						{
							foreach (var error in _session.Raven.Query<ErrorDocument, Errors>()
								.Skip(i * 25)
                                .Take(25)
                                .OrderBy(e => e.FriendlyId)
								.As<Error>())
							{
								if (_session.Raven.Load<Issue>(error.IssueId) == null)
								{
									_errorDeletedCount++;
									_session.Raven.Delete(error);
								}
								else
								{
									ProcessError(error);
								}
							}

							_session.Commit();
							_session.Close();
                            new SynchroniseIndexCommitAction<Errors>().Execute(_session);
							Trace("Committed page {0}, processed {1}, ignored {2}, deleted {3}", i, _errorCount, _errorIgnoredCount, _errorDeletedCount);
						}
					}
				}
			}

			session.Commit();
			return Content("Done");
		}

		private void ProcessError(Error error)
		{
			if (error.ExceptionInfos == null || 
				error.ExceptionInfos.Length == 0 || 
				error.ContextData != null || 
				(error.ContextData != null && error.ContextData.Count > 0) ||
				error.ExceptionInfos[0].ExtraData == null)
			{
				_errorIgnoredCount++;
				return;
			}

			_errorCount++;
			error.ContextData = new Dictionary<string, string>();

			var exceptionData = error.ExceptionInfos[0].ExtraData.Where(s => s.Key.StartsWith("Exception"));
			var contextData = error.ExceptionInfos[0].ExtraData.Where(s => !s.Key.StartsWith("Exception"));

			foreach (var dataItem in contextData)
			{
				error.ContextData.Add(dataItem.Key, dataItem.Value);
			}

			error.ExceptionInfos[0].ExtraData = exceptionData.ToDictionary(s => s.Key, s => s.Value);
		}

		[HttpGet, ExportViewData]
		public ActionResult SyncIndexes(string orgId)
		{
			Server.ScriptTimeout = 7200; //timeout in 2 hours

			var masterDocumentStore = _storeFactory.Create(RavenInstance.Master());

			if (orgId.IsNullOrEmpty())
			{
				Trace("Syncing Errordite Indexes");
				IndexCreation.CreateIndexes(new CompositionContainer(
					new AssemblyCatalog(typeof(Issues).Assembly), new ExportProvider[0]),
					masterDocumentStore.DatabaseCommands.ForDatabase(CoreConstants.ErrorditeMasterDatabaseName),
					masterDocumentStore.Conventions);
				Trace("Done Syncing Errordite Indexes");
			}

			foreach (var organisation in Core.Session.MasterRaven.Query<Organisation>().GetAllItemsAsList(100))
			{
				if (orgId.IsNotNullOrEmpty() && organisation.FriendlyId == orgId)
					continue;

				organisation.RavenInstance = Core.Session.MasterRaven.Load<RavenInstance>(organisation.RavenInstanceId);

				using (_session.SwitchOrg(organisation))
				{
					Trace("Syncing {0} Indexes", organisation.Name);
					_session.BootstrapOrganisation(organisation);
					Trace("Done Syncing {0} Indexes", organisation.Name);
				}
			}

			ConfirmationNotification("All indexes for all organisations have been updated");
			return RedirectToAction("index");
		}

        public ActionResult ClearCache()
        {
            ObjectFactory.GetObject<ICacheEngine>(CacheEngines.Memory).Clear();
            return Content("OK");
        }

        [ImportViewData]
        public ActionResult Index()
        {
            if (AppContext.AuthenticationStatus != AuthenticationStatus.Anonymous)
                return Redirect(Url.Dashboard());

            return View();
        }

        [HttpGet, ImportViewData]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost, ExportViewData]
        public ActionResult Contact(ContactUsViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "contact");
            }

            var emailInfo = new NonTemplatedEmailInfo
            {
                To = _configuration.AdministratorsEmail,
                Subject = "Errordite: Contact Us",
                Body =
                    @"Message from: '{0}'<br />
                        <!--Contact Reason: '{1}'<br />-->
                        Email Address: '{2}'<br /><br />
                        Message: {3}"
                    .FormatWith(viewModel.Name, viewModel.Reason, viewModel.Email, viewModel.Message)
            };

            Core.Session.AddCommitAction(new SendMessageCommitAction(emailInfo, _configuration.GetNotificationsQueueAddress()));

            ConfirmationNotification(Resources.Home.MessageReceived);
            return RedirectToAction("contact");
        }
    }
}
