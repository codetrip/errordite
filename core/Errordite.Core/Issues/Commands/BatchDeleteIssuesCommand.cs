using System.Collections.Generic;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using System.Linq;

namespace Errordite.Core.Issues.Commands
{
    public class BatchBatchDeleteIssuesCommand : SessionAccessBase, IBatchDeleteIssuesCommand
    {
        private readonly IDeleteIssueCommand _deleteIssueCommand;

        public BatchBatchDeleteIssuesCommand(IDeleteIssueCommand deleteIssueCommand)
        {
            _deleteIssueCommand = deleteIssueCommand;
        }

        public BatchDeleteIssuesResponse Invoke(BatchDeleteIssuesRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            var issueIds = request.IssueIds.Select(i => Issue.GetId(i.Split('|')[0])).ToList();

            foreach (var issueId in issueIds)
            {
                _deleteIssueCommand.Invoke(new DeleteIssueRequest
                {
                    CurrentUser = request.CurrentUser,
                    IssueId = issueId,
                    IsBatchDelete = true
                });
            }

			Session.AddCommitAction(new RaiseIssueDeletedEvent(string.Join("^", request.IssueIds)));

            return new BatchDeleteIssuesResponse();
        }
    }

    public interface IBatchDeleteIssuesCommand : ICommand<BatchDeleteIssuesRequest, BatchDeleteIssuesResponse>
    { }

    public class BatchDeleteIssuesResponse
    {}

    public class BatchDeleteIssuesRequest : OrganisationRequestBase
    {
        public IEnumerable<string> IssueIds { get; set; }
    }
}
