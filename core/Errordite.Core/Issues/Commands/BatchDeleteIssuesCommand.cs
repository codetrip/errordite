using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Raven.Abstractions.Data;
using System.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Commands
{
    public class BatchBatchDeleteIssuesCommand : SessionAccessBase, IBatchDeleteIssuesCommand
    {
        private readonly IDeleteErrorsCommand _deleteErrorsCommand;

        public BatchBatchDeleteIssuesCommand(IDeleteErrorsCommand deleteErrorsCommand)
        {
            _deleteErrorsCommand = deleteErrorsCommand;
        }

        public BatchDeleteIssuesResponse Invoke(BatchDeleteIssuesRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            var issueIds = request.IssueIds.Select(i => Issue.GetId(i.Split('|')[0])).ToList();

            _deleteErrorsCommand.Invoke(new DeleteErrorsRequest
            {
                IssueIds = issueIds,
                CurrentUser = request.CurrentUser
            });

            foreach (var partition in issueIds.Partition(25))
            {
                Session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.Issues, new IndexQuery
                {
                    Query = "({0})".FormatWith(partition.ToRavenQuery("Id"))
                });
            }

			foreach (var issueId in issueIds)
			{
				//re-initialise the hourly count document
				Session.Raven.Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(issueId.GetFriendlyId())).Initialise();
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
