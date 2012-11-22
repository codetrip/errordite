using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Web.Mvc;
using System.Linq;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Notifications.Naming;
using Errordite.Core.Notifications.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionSelectors;

namespace Errordite.Web.Controllers
{
    [Authorize, RoleAuthorize(UserRole.SuperUser)]
    public class EmailTestController : ErrorditeController
    {
        private readonly IGetEmailInfoQuery _getEmailInfoQuery;
        private readonly ISendEmailCommand _sendEmailCommand;
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly IEmailNamingMapper _mapper;

        public EmailTestController(IGetEmailInfoQuery getEmailInfoQuery, ISendEmailCommand sendEmailCommand, ISendNotificationCommand sendNotificationCommand, IEmailNamingMapper mapper)
        {
            _getEmailInfoQuery = getEmailInfoQuery;
            _sendEmailCommand = sendEmailCommand;
            _sendNotificationCommand = sendNotificationCommand;
            _mapper = mapper;
        }


        public ActionResult List()
        {
            var emails =
                from t in typeof(EmailInfoBase).Assembly.GetTypes()
                where (t.IsSubclassOf(typeof(EmailInfoBase)))
                select _mapper.InfoToName(t);

            return View(emails);
        }

        public ActionResult Index(string id)
        {
            var emailInfo = _getEmailInfoQuery.Invoke(new GetEmailInfoQueryRequest
            {
                EmailName = id
            });

            return View(emailInfo);
        }

        [ActionName("Generate"), IfButtonClicked("SendInProcess")]
        public ActionResult SendInProcess(string emailName, FormCollection formCollection)
        {
            Debug.Assert(_sendEmailCommand != null, "_sendEmailCommand != null");
            var emailInfo = GetEmailInfo(emailName, formCollection);
            _sendEmailCommand.Invoke(new SendEmailRequest
                                         {
                                             EmailInfo = emailInfo
                                         });

            return RedirectToAction("Index", new { id = emailName });
        }

        [ActionName("Generate"), IfButtonClicked("SendViaService")]
        public ActionResult SendViaService(string emailName, FormCollection formCollection)
        {
            var emailInfo = GetEmailInfo(emailName, formCollection);

            _sendNotificationCommand.Invoke(new SendNotificationRequest()
                                          {
                                              EmailInfo = emailInfo
                                          });

            return RedirectToAction("Index", new { id = emailName });
        }

        [ActionName("Generate"), IfButtonClicked("View")]
        public ActionResult View(string emailName, FormCollection formCollection)
        {
            var emailInfo = GetEmailInfo(emailName, formCollection);

            var email = _sendEmailCommand.Invoke(new SendEmailRequest()
                                                     {
                                                         EmailInfo = emailInfo,
                                                         SkipSend = true,
                                                     }).Message;

            return Content(email.Body);
        }

        private EmailInfoBase GetEmailInfo(string emailName, NameValueCollection formCollection)
        {
            var emailInfoResponse = _getEmailInfoQuery.Invoke(new GetEmailInfoQueryRequest { EmailName = emailName });

            var emailInfo = (EmailInfoBase)Activator.CreateInstance(emailInfoResponse.EmailInfoType);

            var paramInfo = emailInfoResponse.Parameters;

            foreach (var propName in formCollection.AllKeys.Where(x => !"EmailName".Equals(x, StringComparison.InvariantCultureIgnoreCase)).Select(paramName => paramName.Split('.')[0]))
            {
                EmailParameterType parameterType;
                if (!paramInfo.TryGetValue(propName, out parameterType))
                    continue;

                switch (parameterType)
                {
                    case EmailParameterType.Bool:
                        emailInfo.GetType().GetProperty(propName).SetValue(emailInfo,
                                                                           formCollection[propName].StartsWith("true"),
                                                                           null);
                        break;
                    case EmailParameterType.String:
                        emailInfo.GetType().GetProperty(propName).SetValue(emailInfo, formCollection[propName], null);
                        break;
                    case EmailParameterType.Int:
                        SetPossiblyNullableProperty(emailInfo, propName, formCollection[propName], int.Parse);
                        break;
                    case EmailParameterType.Decimal:
                        SetPossiblyNullableProperty(emailInfo, propName, formCollection[propName], decimal.Parse);
                        break;
                    case EmailParameterType.DateTime:
                        SetPossiblyNullableProperty(emailInfo, propName, formCollection[propName], DateTime.Parse);
                        break;
                    case EmailParameterType.Guid:
                        SetPossiblyNullableProperty(emailInfo, propName, formCollection[propName], Guid.Parse);
                        break;
                    case EmailParameterType.List:
                        emailInfo.GetType().GetProperty(propName).SetValue(emailInfo,
                                                                           formCollection[propName].Split('|').ToList(),
                                                                           null);
                        break;
                }
            }

            return emailInfo;
        }

        private void SetPossiblyNullableProperty<TProp>(object o, string paramName, string paramValue, Func<string, TProp> converter) where TProp : struct
        {
            TProp? value = null;
            try
            {
                value = converter(paramValue);
            }
            catch
            {
            }

            o.GetType().GetProperty(paramName).SetValue(o, value, null);
        }
    }
}