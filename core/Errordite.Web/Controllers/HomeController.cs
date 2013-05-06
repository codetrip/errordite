﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Text;
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
using HtmlAgilityPack;
using Raven.Client;
using Raven.Client.Indexes;
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
				_session.MasterRaven.Delete(mapping);
			}

            session.Commit();
            return Content("Done");
        }

		[HttpGet, ImportViewData]
		public ActionResult SetOrg()
		{
			var session = ObjectFactory.GetObject<IAppSession>();

			var ravenInstances = session.MasterRaven.Query<RavenInstance>().ToList();

			foreach (var organisation in session.MasterRaven.Query<Organisation, Organisations>())
			{
				organisation.RavenInstance = ravenInstances.First(r => r.Id == organisation.RavenInstanceId);

				using (_session.SwitchOrg(organisation))
				{
					foreach (var user in _session.Raven.Query<User, Users>())
					{
						user.OrganisationId = organisation.Id;
					}
				}
			}

			session.Commit();
			return Content("Done");
		}

		[HttpGet, ImportViewData]
		public ActionResult Users()
		{
			var session = ObjectFactory.GetObject<IAppSession>();

			var ravenInstances = session.MasterRaven.Query<RavenInstance>().ToList();

			foreach (var organisation in session.MasterRaven.Query<Organisation, Organisations>())
			{
				organisation.RavenInstance = ravenInstances.First(r => r.Id == organisation.RavenInstanceId);

				using (_session.SwitchOrg(organisation))
				{
					foreach (var user in _session.Raven.Query<User, Users>())
					{
						_session.MasterRaven.Store(new UserOrganisationMapping
						{
							EmailAddress = user.Email,
							Organisations = new List<string> { organisation.Id}
						});
					}
				}
			}

			session.Commit();
			return Content("Done");
		}

		[HttpGet, ImportViewData]
		public ActionResult Html()
		{
			Server.ScriptTimeout = 7200; //timeout in 2 hours

			var session = ObjectFactory.GetObject<IAppSession>();

			var ravenInstances = session.MasterRaven.Query<RavenInstance>().ToList();

			foreach (var organisation in session.MasterRaven.Query<Organisation, Organisations>())
			{
				organisation.RavenInstance = ravenInstances.First(r => r.Id == organisation.RavenInstanceId);

				using (_session.SwitchOrg(organisation))
				{
					RavenQueryStatistics stats;

					foreach(var error in _session.Raven.Query<Core.Domain.Error.Error, Errors>().Statistics(out stats)
						.Skip(0)
						.Take(25)
						.As<Core.Domain.Error.Error>()
						.ToList())
					{
						Trace("Processing Error:={0}", error.Id);
						foreach (var info in error.ExceptionInfos)
						{
							info.Message = info.Message.RemoveHtml();
							info.StackTrace = info.StackTrace.RemoveHtml();
						}
					}

					_session.Commit();
					_session.Close();

					if (stats.TotalResults > 25)
					{
						int pageNumber = stats.TotalResults / 25;

						for (int i = 1; i < pageNumber; i++)
						{
							foreach(var error in _session.Raven.Query<Core.Domain.Error.Error, Errors>()
								.Skip(i * 25)
								.Take(25)
								.As<Core.Domain.Error.Error>())
							{
								Trace("Processing Error:={0}", error.Id);

								foreach (var info in error.ExceptionInfos)
								{
									info.Message = info.Message.RemoveHtml();
									info.StackTrace = info.StackTrace.RemoveHtml();
								}
							}

							_session.Commit();
							_session.Close();
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

			Trace("Syncing Errordite Indexes");
			IndexCreation.CreateIndexes(new CompositionContainer(
				new AssemblyCatalog(typeof(Issues).Assembly), new ExportProvider[0]),
				masterDocumentStore.DatabaseCommands.ForDatabase(CoreConstants.ErrorditeMasterDatabaseName),
				masterDocumentStore.Conventions);

			Trace("Done Syncing Errordite Indexes");
			foreach (var organisation in Core.Session.MasterRaven.Query<Organisation>().GetAllItemsAsList(100))
			{
				organisation.RavenInstance = Core.Session.MasterRaven.Load<RavenInstance>(organisation.RavenInstanceId);

				using (_session.SwitchOrg(organisation))
				{
					Trace("Done Syncing {0} Indexes", organisation.Name);
					_session.BootstrapOrganisation(organisation);
					Trace("Syncing {0} Indexes", organisation.Name);
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
