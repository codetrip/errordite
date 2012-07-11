
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Session;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Reception.Commands;

namespace Errordite.Reception.Service.Controllers
{

    public class ErrorsController : ApiController
    {
        private readonly IAppSession _session;
        private readonly IComponentAuditor _auditor;
        private readonly IReprocessIssueErrorsCommand _reprocessIssueErrorsCommand;

        public ErrorsController()
        {
            _auditor = ObjectFactory.GetObject<IComponentAuditor>();
            _session = ObjectFactory.GetObject<IAppSession>();
            _reprocessIssueErrorsCommand = ObjectFactory.GetObject<IReprocessIssueErrorsCommand>();
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="request"></param>
        public HttpResponseMessage PostErrors(ReprocessIssueErrorsRequest request)
        {
            try
            {
                _auditor.Trace(GetType(), "Received reprocess issue errors request, IssueId:={0}, ", request.IssueId);
                var response = _reprocessIssueErrorsCommand.Invoke(request);
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
