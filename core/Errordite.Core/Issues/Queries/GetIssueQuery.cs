using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Session;

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
