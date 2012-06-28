using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues;
using System.Linq;

namespace Errordite.Reception.Service.Controllers
{
    public class IssueController : ApiController
    {
        private readonly IComponentAuditor _auditor;
        private readonly IReceptionServiceIssueCache _issueCache;

        public IssueController()
        {
            _auditor = ObjectFactory.GetObject<IComponentAuditor>();
            _issueCache = ObjectFactory.GetObject<IReceptionServiceIssueCache>();
        }

        public IssueBase Get(string id, string applicationId)
        {
            _auditor.Trace(GetType(), "Request for issue with Id:={0}, ApplicationId:={1}", id, applicationId);
            var issue = _issueCache.GetIssues(applicationId).FirstOrDefault(i => i.Id == Issue.GetId(id));
            _auditor.Trace(GetType(), "Issue {0}", issue == null ? "not found" : "found");
            return issue;
        }

        /// <summary>
        /// Update an issue
        /// </summary>
        /// <param name="issues"></param>
        public void PutIssue(IEnumerable<IssueBase> issues)
        {
            foreach(var issue in issues)
            {
                _auditor.Trace(GetType(), "Request to put issue with Id:={0}, ApplicationId:={1}", issue.Id, issue.ApplicationId);
                _issueCache.Update(issue);
            }
        }

        /// <summary>
        /// Add an issue
        /// </summary>
        /// <param name="issue"></param>
        public HttpResponseMessage PostIssue(IssueBase issue)
        {
            _auditor.Trace(GetType(), "Request to create issue with Id:={0}, ApplicationId:={1}", issue.Id, issue.ApplicationId);
            _issueCache.Add(issue);

            return Request.CreateResponse(HttpStatusCode.Created, issue);
        }

        public HttpResponseMessage DeleteIssue(string id)
        {
           _auditor.Trace(GetType(), "Incoming Ids:={0}", id);

            string[] issueIds = id.Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string issueId in issueIds)
            {
                string[] idParts = issueId.Split('|');
                _auditor.Trace(GetType(), "Request to delete issue with Id:={0}, ApplicationId:={1}", idParts[0], idParts[1]);
                _issueCache.Delete(idParts[0], idParts[1]);
            }
            
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
