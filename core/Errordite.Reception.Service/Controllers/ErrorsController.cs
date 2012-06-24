
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Session;
using Errordite.Core.Reception.Commands;
using System.Linq;

namespace Errordite.Reception.Service.Controllers
{
    public class ErrorsController : ApiController
    {
        private readonly IAppSession _session;
        private readonly IComponentAuditor _auditor;
        private readonly IReceiveErrorCommand _receiveErrorCommand;

        public ErrorsController()
        {
            _auditor = ObjectFactory.GetObject<IComponentAuditor>();
            _receiveErrorCommand = ObjectFactory.GetObject<IReceiveErrorCommand>();
            _session = ObjectFactory.GetObject<IAppSession>();
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="errors"></param>
        public HttpResponseMessage PostErrors(IEnumerable<ReceiveErrorRequest> errors)
        {
            _auditor.Trace(GetType(), "Request process {0} errors", errors.Count());

            var responses = errors.Select(error => _receiveErrorCommand.Invoke(error)).ToList();

            _session.Commit();

            return Request.CreateResponse(HttpStatusCode.OK, responses);
        }
    }
}
