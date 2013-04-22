using System.Net;
using System.Net.Http;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Web;

namespace Errordite.Services.Controllers
{
    public class OrganisationController : ErrorditeApiController
    {
        private readonly IErrorditeService _errorditeService;

        public OrganisationController(IErrorditeService errorditeService)
        {
            _errorditeService = errorditeService;
        }

        public HttpResponseMessage Post(string orgId, Organisation organisation)
        {
            _errorditeService.AddOrganisation(organisation);
            return Request.CreateResponse(HttpStatusCode.OK, organisation);
        }
    }
}