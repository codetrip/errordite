using System;
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
using Errordite.Core.Messaging;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Home;
using Errordite.Core.Extensions;
using Errordite.Web.Extensions;
using Errordite.Core.Web;
using Raven.Client.Indexes;

namespace Errordite.Web.Controllers
{
    public class HomeController : ErrorditeController
    {
        private readonly ErrorditeConfiguration _configuration;

        public HomeController(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet, ImportViewData]
        public ActionResult Test()
        {
            var session = ObjectFactory.GetObject<IAppSession>();

            for (int i = 0; i < 5; i++)
            {
                session.MasterRaven.Store(new MessageEnvelope
                {
                    Message = "test",
                    QueueUrl = "test",
                    Service = Service.Receive,
                    ErrorMessage = "This is a test error message",
                    GeneratedOnUtc = DateTime.UtcNow,
                    MessageType = "message type",
                    OrganisationId = "Organisations/1"
                });
            }

            for (int i = 0; i < 5; i++)
            {
                session.MasterRaven.Store(new MessageEnvelope
                {
                    Message = "test",
                    QueueUrl = "test",
                    Service = Service.Notifications,
                    ErrorMessage = "This is a test error message",
                    GeneratedOnUtc = DateTime.UtcNow,
                    MessageType = "message type",
                    OrganisationId = "Organisations/1"
                });
            }

            for (int i = 0; i < 5; i++)
            {
                session.MasterRaven.Store(new MessageEnvelope
                {
                    Message = "test",
                    QueueUrl = "test",
                    Service = Service.Events,
                    ErrorMessage = "This is a test error message",
                    GeneratedOnUtc = DateTime.UtcNow,
                    MessageType = "message type",
                    OrganisationId = "Organisations/1"
                });
            }

            session.Commit();
            return Content("Done");
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
