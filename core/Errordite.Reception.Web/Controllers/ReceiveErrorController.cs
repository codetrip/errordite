using System.Net;
using System.Web.Mvc;
using Errordite.Core.Auditing;
using Errordite.Core.Reception.Commands;
using Errordite.Core.Extensions;

namespace Errordite.Reception.Web.Controllers
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

            if (response.ResponseMessage.IsNotNullOrEmpty())
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return Content(response.ResponseMessage);
            }

            Response.StatusCode = (int) HttpStatusCode.Created;
            return Content("Received error");
        }
    }
}
