//using System;
//using System.Web.Mvc;
//using CodeTrip.Core.Extensions;
//using Errordite.Core.Alerts;
//using Errordite.Web.ActionResults;
//using System.Linq;
//using Errordite.Core.Extensions;

//namespace Errordite.Web.Controllers
//{
//    public class AlertsController : ErrorditeController
//    {
//        private readonly IUserAlertsSeenCommand _userAlertsSeenCommand;
//        private readonly IGetUserAlertsQuery _getUsersAlerts;

//        public AlertsController(IUserAlertsSeenCommand userAlertsSeenCommand, IGetUserAlertsQuery getUsersAlerts)
//        {
//            _userAlertsSeenCommand = userAlertsSeenCommand;
//            _getUsersAlerts = getUsersAlerts;
//        }

//        public ActionResult Dismiss(string id)
//        {
//            _userAlertsSeenCommand.Invoke(
//                new UserAlertsSeenRequest
//                    {
//                        AlertIds = new[] {id}, 
//                        CurrentUser = AppContext.CurrentUser
//                    });

//            return new JsonSuccessResult();
//        }

//        public ActionResult DismissAll()
//        {
//            _userAlertsSeenCommand.Invoke(
//                new UserAlertsSeenRequest
//                {
//                    AlertIds = null,
//                    CurrentUser = AppContext.CurrentUser
//                });

//            return new JsonSuccessResult();
//        }

//        [HttpGet]
//        public ActionResult Get()
//        {
//            var alerts = _getUsersAlerts.Invoke(new GetUserAlertsRequest { UserId = Core.AppContext.CurrentUser.Id }).Alerts;

//            return new JsonSuccessResult(alerts.Select(a => new
//            {
//                a.Id,
//                Message = a.Message.FormatWith(a.Replacements),
//                Header = a.Type.IsNullOrEmpty() ? string.Empty : (Resources.Alerts.ResourceManager.GetString(a.Type) ?? "Alert"),
//                Date = a.SentUtc.ToLocalTimeFormatted(),
//            }), true);
//        }
//    }
//}