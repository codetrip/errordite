using System.Net;
using System.Web.Mvc;
using Errordite.Core.Auditing;
using Errordite.Core.Reception.Commands;
using Errordite.Core.Extensions;

namespace Errordite.Receive.Controllers
{
    public class ReceiveErrorController : AuditingController
    {
        private readonly IProcessIncomingExceptionCommand _processIncomingException;

        public ReceiveErrorController(IProcessIncomingExceptionCommand processIncomingException)
        {
            _processIncomingException = processIncomingException;
        }

        [HttpPost]
        public ActionResult Index(Client.ClientError clientError)
        {
            var response = _processIncomingException.Invoke(new ProcessIncomingExceptionRequest
            {
                Error = clientError
            });

            Response.StatusCode = (int)response.ResponseCode;

            if (response.ResponseMessage.IsNotNullOrEmpty())
            {
                return Content(response.ResponseMessage);
            }

            return Content("Received error");
        }
    }
}
