using System;
using System.Net;
using System.Net.Http;
using Errordite.Core.Reception.Commands;
using Errordite.Core.Web;

namespace Errordite.Services.Controllers
{
    public class ErrorController : ErrorditeApiController
    {
        private readonly IReceiveErrorCommand _receiveErrorCommand;

        public ErrorController(IReceiveErrorCommand receiveErrorCommand)
        {
            _receiveErrorCommand = receiveErrorCommand;
        }

        public HttpResponseMessage Post(string orgId, ReceiveErrorRequest request)
        {
            try
            {
                SetOrganisation(orgId);
                Auditor.Trace(GetType(), "Started...");
                var response = _receiveErrorCommand.Invoke(request);
                Session.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                Auditor.Error(GetType(), e);
                throw;
            }
        }
    }
}