using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Raven.Abstractions.Data;

namespace Errordite.Core.Issues.Commands
{
    public class DeleteIssueCommand : SessionAccessBase, IDeleteIssueCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public DeleteIssueCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public DeleteIssueResponse Invoke(DeleteIssueRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            var issueId = Issue.GetId(request.IssueId);
            var issue = Load<Issue>(issueId);

            if(issue == null)
            {
                return new DeleteIssueResponse
                {
                    Status = DeleteIssueStatus.IssueNotFound
                };
            }

            _authorisationManager.Authorise(issue, request.CurrentUser);

			//delete the issues errors
            Session.AddCommitAction(new DeleteAllErrorsCommitAction(issue.Id));
            Session.AddCommitAction(new DeleteAllDailyCountsCommitAction(issue.Id));

			//delete the hourly count doc
			Delete(Session.Raven.Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(issue.FriendlyId)));

			//and delete the issue
            Delete(issue);

            if (!request.IsBatchDelete)
            {
                //tell the reception service an issue has been deleted
                Session.AddCommitAction(new RaiseIssueDeletedEvent("{0}|{1}".FormatWith(issue.FriendlyId, IdHelper.GetFriendlyId(issue.ApplicationId))));
            }

            return new DeleteIssueResponse
            {
                Status = DeleteIssueStatus.Ok
            };
        }
    }

    public interface IDeleteIssueCommand : ICommand<DeleteIssueRequest, DeleteIssueResponse>
    { }

    public class DeleteIssueResponse
    {
        public DeleteIssueStatus Status { get; set; }
    }

    public class DeleteIssueRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public bool IsBatchDelete { get; set; }
    }

    public enum DeleteIssueStatus
    {
        Ok,
        IssueNotFound
    }
}
