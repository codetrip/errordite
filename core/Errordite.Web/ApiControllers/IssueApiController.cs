using System.Net;
using System.Net.Http;
using System.Web.Http;
using Errordite.Core.Extensions;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Web;

namespace Errordite.Web.ApiControllers
{
    public class IssueApiController : ErrorditeApiController
    {
        private readonly IAddIssueCommand _addIssueCommand;

        public IssueApiController(IAddIssueCommand addIssueCommand)
        {
            _addIssueCommand = addIssueCommand;
        }

        public Issue GetIssue(string id, [FromUri]string apiKey)
        {
            var organisation = GetOrganisationFromApiKey(apiKey);

            if (organisation == null)
            {
                Auditor.Trace(GetType(), "...Failed to authenticate with apiKey:={0}", apiKey);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            var issue = Session.Raven.Load<Issue>(Issue.GetId(id));

            //make sure we dont allow someone else's issue to be accessed
            if (issue == null || issue.OrganisationId != organisation.Id)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            return new Issue
            {
                Id = issue.Id,
                Name = issue.Name,
                NotifyFrequency = issue.NotifyFrequency,
                ApplicationId = issue.ApplicationId,
                OrganisationId = issue.OrganisationId,
                Status = issue.Status,
                CreatedOnUtc = issue.CreatedOnUtc,
                ErrorCount = issue.ErrorCount,
                LastErrorUtc = issue.LastErrorUtc,
                LastModifiedUtc = issue.LastModifiedUtc,
                LastRuleAdjustmentUtc = issue.LastRuleAdjustmentUtc,
                Reference = issue.Reference,
                Rules = issue.Rules
            };
        }

        public HttpResponseMessage PutIssue(string id, [FromUri]string apiKey, [FromBody]Issue issue)
        {
            Auditor.Trace(GetType(), "Started...");
            Auditor.Trace(GetType(), "...Attempting to load application from apiKey:={0}", apiKey);

            var organisation = GetOrganisationFromApiKey(apiKey);

            if (organisation == null)
            {
                Auditor.Trace(GetType(), "...Failed to authenticate with apiKey:={0}", apiKey);
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            Auditor.Trace(GetType(), "...Successfully loaded organisation Name:={0}, Id:={1}", organisation.Name, organisation.Id);

            var storedIssue = Session.Raven.Load<Issue>(Issue.GetId(id));

            if (storedIssue == null || storedIssue.OrganisationId != organisation.Id)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (issue.Name.IsNotNullOrEmpty())
                storedIssue.Name = issue.Name;

            if (issue.Reference.IsNotNullOrEmpty())
                storedIssue.Reference = issue.Reference;

            storedIssue.Status = issue.Status;
            storedIssue.NotifyFrequency = issue.NotifyFrequency;

            return Request.CreateResponse(HttpStatusCode.OK, new Issue
            {
                Id = storedIssue.Id,
                Name = storedIssue.Name,
                NotifyFrequency = storedIssue.NotifyFrequency,
                ApplicationId = storedIssue.ApplicationId,
                OrganisationId = storedIssue.OrganisationId,
                Status = storedIssue.Status,
                CreatedOnUtc = storedIssue.CreatedOnUtc,
                ErrorCount = storedIssue.ErrorCount,
                LastErrorUtc = storedIssue.LastErrorUtc,
                LastModifiedUtc = storedIssue.LastModifiedUtc,
                LastRuleAdjustmentUtc = storedIssue.LastRuleAdjustmentUtc,
                Reference = storedIssue.Reference,
                Rules = storedIssue.Rules
            });
        }
    }
}
