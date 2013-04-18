using System.Web.Mvc;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.Caching.Resources;
using CodeTrip.Core.IoC;
using Errordite.Core.Configuration;
using Errordite.Core.Identity;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Session;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Home;
using CodeTrip.Core.Extensions;
using Errordite.Web.Extensions;

namespace Errordite.Web.Controllers
{
    public class HomeController : ErrorditeController
    {
        private readonly ErrorditeConfiguration _configuration;

        public HomeController(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ActionResult ClearCache()
        {
            ObjectFactory.GetObject<ICacheEngine>(CacheEngines.RedisMemoryHybrid).Clear();
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

            Core.Session.AddCommitAction(new SendNServiceBusMessage("Send {0}".FormatWith(emailInfo.GetType().Name), emailInfo, _configuration.NotificationsQueueName));

            ConfirmationNotification(Resources.Home.MessageReceived);
            return RedirectToAction("contact");
        }
    }
}
