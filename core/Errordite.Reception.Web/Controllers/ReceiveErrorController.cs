using System.Web.Mvc;
using CodeTrip.Core.Auditing;
using Errordite.Core.Reception.Commands;

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
        public ActionResult Index(Client.Abstractions.ClientError clientError)
        {
            _processIncomingException.Invoke(new ProcessIncomingExceptionRequest
            {
                Error = clientError
            });

            return Content("Ok");
        }
    }
}
