using System.Web.Mvc;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.Caching.Resources;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.IoC;
using Errordite.Core.Configuration;
using Errordite.Core.Identity;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Home;
using Errordite.Core.Extensions;
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
				organisation.Subscription = new Subscription
				{
					Dispensation = true,
					Status = SubscriptionStatus.Trial
				};
				organisation.PaymentPlanId = "PaymentPlans/1";
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
