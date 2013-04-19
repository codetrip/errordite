using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues;
using System.Linq;
using Errordite.Core.Web;

namespace Errordite.Services.Controllers
{
    public class IssueController : ErrorditeApiController
    {
        private readonly IReceptionServiceIssueCache _issueCache;

        public IssueController(IReceptionServiceIssueCache issueCache)
        {
            _issueCache = issueCache;
        }

        public IssueBase Get(string orgId, string id, string applicationId)
        {
            SetOrganisation(orgId);
            Auditor.Trace(GetType(), "Request for issue with Id:={0}, ApplicationId:={1}", id, applicationId);
            var issue = _issueCache.GetIssues(applicationId, orgId).FirstOrDefault(i => i.Id == Issue.GetId(id));
            Auditor.Trace(GetType(), "Issue {0}", issue == null ? "not found" : "found");
            return issue;
        }

        /// <summary>
        /// Update an issue
        /// </summary>
        /// <param name="orgId"> </param>
        /// <param name="issues"></param>
        public void PutIssue(string orgId, IEnumerable<IssueBase> issues)
        {
            SetOrganisation(orgId);
            
            foreach(var issue in issues)
            {
                Auditor.Trace(GetType(), "Request to put issue with Id:={0}, ApplicationId:={1}, RuleCount:={2}", issue.Id, issue.ApplicationId, issue.Rules.Count);
                _issueCache.Update(issue);
            }
        }

        /// <summary>
        /// Add an issue
        /// </summary>
        /// <param name="orgId"> </param>
        /// <param name="issue"></param>
        public HttpResponseMessage PostIssue(string orgId, IssueBase issue)
        {
            SetOrganisation(orgId);
            Auditor.Trace(GetType(), "Request to create issue with Id:={0}, ApplicationId:={1}", issue.Id, issue.ApplicationId);
            _issueCache.Add(issue);

            return Request.CreateResponse(HttpStatusCode.Created, issue);
        }

        public HttpResponseMessage DeleteIssue(string orgId, string id)
        {
            Auditor.Trace(GetType(), "Incoming Ids:={0}", id);
            SetOrganisation(orgId);
            string[] issueIds = id.Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string issueId in issueIds)
            {
                string[] idParts = issueId.Split('|');
                Auditor.Trace(GetType(), "Request to delete issue with Id:={0}, ApplicationId:={1}", idParts[0], idParts[1]);
                _issueCache.Delete(idParts[0], idParts[1], orgId);
            }
            
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
