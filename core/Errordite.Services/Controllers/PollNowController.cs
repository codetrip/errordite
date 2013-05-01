using System.Net;
using System.Net.Http;
using Errordite.Core.Web;
using Errordite.Core.Extensions;

namespace Errordite.Services.Controllers
{
    public class PollNowController : ErrorditeApiController
    {
        private readonly IErrorditeService _errorditeService;

        public PollNowController(IErrorditeService errorditeService)
        {
            _errorditeService = errorditeService;
        }

        public HttpResponseMessage Post(string orgId)
        {
			_errorditeService.PollNow(orgId.GetFriendlyId());
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}