using System.Net;
using System.Net.Http;
using Errordite.Core.Web;

namespace Errordite.Services.Controllers
{
    public class PollNowController : ErrorditeApiController
    {
        private readonly IErrorditeService _errorditeService;

        public PollNowController(IErrorditeService errorditeService)
        {
            _errorditeService = errorditeService;
        }

        public HttpResponseMessage Post(string organisationId)
        {
            _errorditeService.PollNow(organisationId);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}