
using System;
using System.Net;
using System.Net.Http;
using CodeTrip.Core.IoC;
using Errordite.Core.Issues.Commands;

namespace Errordite.Reception.Service.Controllers
{
    public class ReprocessIssueErrorsController : ErrorditeApiController
    {
        private readonly IReprocessIssueErrorsCommand _reprocessIssueErrorsCommand;

        public ReprocessIssueErrorsController()
        {
            _reprocessIssueErrorsCommand = ObjectFactory.GetObject<IReprocessIssueErrorsCommand>();
        }

        public HttpResponseMessage Post(string orgId, ReprocessIssueErrorsRequest request)
        {
            try
            {
                SetOrg(orgId);

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
