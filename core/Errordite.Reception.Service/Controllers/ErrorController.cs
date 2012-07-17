using System;
using System.Net;
using System.Net.Http;
using CodeTrip.Core.IoC;
using Errordite.Core.Reception.Commands;

namespace Errordite.Reception.Service.Controllers
{
    public class ErrorController : ErrorditeApiController
    {
        private readonly IReceiveErrorCommand _receiveErrorCommand = ObjectFactory.GetObject<IReceiveErrorCommand>();
        
        public HttpResponseMessage Post(ReceiveErrorRequest request)
        {
            try
            {
                _auditor.Trace(GetType(), "Started...");
                var response =
                    _receiveErrorCommand.Invoke(request);
                _session.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                _auditor.Error(GetType(), e);
                throw;
            }
        }
    }
}