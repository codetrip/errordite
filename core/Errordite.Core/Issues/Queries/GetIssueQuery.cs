using System.Linq;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Issues.Queries
{
    public class GetIssueQuery : SessionAccessBase, IGetIssueQuery
    {
        private readonly IAuthorisationManager _authorisationManager;

        public GetIssueQuery(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public GetIssueResponse Invoke(GetIssueRequest request)
        {
            Trace("Starting...");

            var issueId = Issue.GetId(request.IssueId);
            var issue = Load<Issue>(issueId);

            if(issue == null)
            {
                Trace("Failed to locate issue with Id:={0}", request.IssueId);
                return new GetIssueResponse();
            }

            _authorisationManager.Authorise(issue, request.CurrentUser);

            var count = Session.Raven.Query<ErrorCountByIssueResult, Errors_CountByIssue>().FirstOrDefault(r => r.IssueId == issueId);

            if (count == null)
                issue.ErrorCount = 0;

            if (count != null && count.Count != issue.ErrorCount)
                issue.ErrorCount = count.Count;
            
            return new GetIssueResponse
            {
                Issue = issue
            };
        }
    }

    public interface IGetIssueQuery : IQuery<GetIssueRequest, GetIssueResponse>
    { }

    public class GetIssueResponse
    {
        public Issue Issue { get; set; }
    }

    public class GetIssueRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
    }
}
