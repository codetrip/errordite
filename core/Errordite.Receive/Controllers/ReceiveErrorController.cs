using System;
using System.Web.Mvc;
using Errordite.Core.Auditing;
using Errordite.Core.Receive.Commands;
using Errordite.Core.Extensions;

namespace Errordite.Receive.Controllers
{
    public class CORSActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.HttpMethod.ToUpperInvariant() == "OPTIONS")
            {
                // do nothing let IIS deal with reply!
                filterContext.Result = new EmptyResult();
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }
    }

    [CORSActionFilter]
    public class ReceiveErrorController : AuditingController
    {
        private readonly IProcessIncomingExceptionCommand _processIncomingException;

        public ReceiveErrorController(IProcessIncomingExceptionCommand processIncomingException)
        {
            _processIncomingException = processIncomingException;
        }
        
        public ActionResult Index(Client.ClientError clientError)
        {
            try
	        {
				var response = _processIncomingException.Invoke(new ProcessIncomingExceptionRequest
				{
					Error = clientError
				});

				Response.StatusCode = response.ResponseCode.HasValue ? (int)response.ResponseCode.Value : response.SpecialResponseCode ?? 200;

				if (response.ResponseMessage.IsNotNullOrEmpty())
				{
					return Content(response.ResponseMessage);
				}

				return Content("Received error");
	        }
	        catch (Exception e)
	        {
		        Response.StatusCode = 500;
		        return Content(e.Message);
	        }
        }
    }
}
