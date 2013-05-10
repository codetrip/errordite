using System;
using System.Net;
using System.Net.Http;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Web;

namespace Errordite.Services.Controllers
{
    public class ReprocessIssueErrorsController : ErrorditeApiController
    {
        private readonly IReprocessIssueErrorsCommand _reprocessIssueErrorsCommand;

        public ReprocessIssueErrorsController(IReprocessIssueErrorsCommand reprocessIssueErrorsCommand)
        {
            _reprocessIssueErrorsCommand = reprocessIssueErrorsCommand;
        }

        public HttpResponseMessage Post(string orgId, ReprocessIssueErrorsRequest request)
        {
            try
            {
                SetOrganisation(orgId);

                Auditor.Trace(GetType(), "Received reprocess issue errors request, IssueId:={0}, ", request.IssueId);
                var response = _reprocessIssueErrorsCommand.Invoke(request);
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
