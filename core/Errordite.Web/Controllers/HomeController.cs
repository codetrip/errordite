using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Web.Mvc;
using Errordite.Core;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.Caching.Resources;
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
using Raven.Client.Indexes;
using Raven.Client.Linq;
using System.Linq;

namespace Errordite.Web.Controllers
{
    public class HomeController : ErrorditeController
    {
		private readonly ErrorditeConfiguration _configuration;
		private readonly IAppSession _session;
		private readonly IShardedRavenDocumentStoreFactory _storeFactory;

        public HomeController(ErrorditeConfiguration configuration, IShardedRavenDocumentStoreFactory storeFactory, IAppSession session)
        {
	        _configuration = configuration;
	        _storeFactory = storeFactory;
	        _session = session;
        }

	    [HttpGet, ImportViewData]
        public ActionResult Test()
        {
	        var session = ObjectFactory.GetObject<IAppSession>();

			foreach (var plan in session.MasterRaven.Query<PaymentPlan>())
			{
				if(plan.IsTrial)
					continue;

				if (plan.Name.ToLowerInvariant() == "small")
				{
					plan.SignUpUrl = "https://code-trip-ltd.chargify.com/h/1182474/subscriptions/new";
					plan.MaximumUsers = 5;
                    plan.MaximumApplications = 2;
                    plan.Price = 19.00m;
				}
				else if (plan.Name.ToLowerInvariant() == "medium")
				{
                    plan.SignUpUrl = "https://code-trip-ltd.chargify.com/h/1182475/subscriptions/new";
                    plan.Price = 79.00m;
				}
				else if (plan.Name.ToLowerInvariant() == "large")
				{
					plan.SignUpUrl = "https://code-trip-ltd.chargify.com/h/1182476/subscriptions/new";
					plan.MaximumUsers = 100;
					plan.MaximumApplications = 25;
				    plan.Price = 199.00m;
				}
			}

			foreach (var organisation in session.MasterRaven.Query<Organisation>())
			{
				var date = DateTime.UtcNow.ToDateTimeOffset(organisation.TimezoneId);
				organisation.Subscription = new Subscription
				{
					Status = SubscriptionStatus.Trial,
					StartDate = date,
					LastModified = date,
					CurrentPeriodEndDate = date.AddMonths(1),
				};
				organisation.PaymentPlanId = "PaymentPlans/1";
			}

			foreach (var mapping in session.MasterRaven.Query<UserOrganisationMapping>())
			{
				mapping.Organisations = new List<string> {mapping.OrganisationId};

				foreach (var organisation in session.MasterRaven.Query<Organisation>())
				{
					using (_session.SwitchOrg(organisation))
					{
						var user = _session.Raven.Query<User, Users>().FirstOrDefault(u => u.Email == mapping.EmailAddress);
						if (user != null)
						{
							mapping.Password = user.Password;
							mapping.PasswordToken = user.PasswordToken;
						}
					}
				}
			}

            session.Commit();
            return Content("Done");
        }

		[HttpGet, ImportViewData]
		public ActionResult Mapping()
		{
			var session = ObjectFactory.GetObject<IAppSession>();
			var ravenInstances = session.MasterRaven.Query<RavenInstance>().ToList();

			foreach (var mapping in session.MasterRaven.Query<UserOrganisationMapping>())
			{
				mapping.Organisations = new List<string> { mapping.OrganisationId };

				foreach (var organisation in session.MasterRaven.Query<Organisation>())
				{
					organisation.RavenInstance = ravenInstances.First(r => r.Id == organisation.RavenInstanceId);

					using (_session.SwitchOrg(organisation))
					{
						var user = _session.Raven.Query<User, Users>().FirstOrDefault(u => u.Email == mapping.EmailAddress);
						if (user != null)
						{
							mapping.Password = user.Password;
							mapping.PasswordToken = user.PasswordToken;
						}
					}
				}
			}

			session.Commit();
			return Content("Done");
		}

		[HttpGet, ExportViewData]
		public ActionResult SyncIndexes()
		{
			Server.ScriptTimeout = 7200; //timeout in 2 hours

			var masterDocumentStore = _storeFactory.Create(RavenInstance.Master());

			IndexCreation.CreateIndexes(new CompositionContainer(
				new AssemblyCatalog(typeof(Issues).Assembly), new ExportProvider[0]),
				masterDocumentStore.DatabaseCommands.ForDatabase(CoreConstants.ErrorditeMasterDatabaseName),
				masterDocumentStore.Conventions);

			foreach (var organisation in Core.Session.MasterRaven.Query<Organisation>().GetAllItemsAsList(100))
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
