using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Errordite.Services.Controllers
{
	public class StopPollingController : ApiController
    {
        private readonly IErrorditeService _errorditeService;

		public StopPollingController(IErrorditeService errorditeService)
        {
            _errorditeService = errorditeService;
        }

        public HttpResponseMessage Post()
        {
			_errorditeService.StopPolling();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}