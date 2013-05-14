using System;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Issues.Commands
{
    public class SetIssueStatusCommand : SessionAccessBase, ISetIssueStatusCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public SetIssueStatusCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public SetIssueStatusResponse Invoke(SetIssueStatusRequest request)
        {
            Trace("Starting...");

            var issue = Load<Issue>(Issue.GetId(request.IssueId));

            if(issue == null)
            {
                return new SetIssueStatusResponse
                {
                    Status = SetIssueStatusStatus.IssueNotFound
                };
            }

            _authorisationManager.Authorise(issue, request.CurrentUser);

            if (issue.Status != request.Status)
            {
                Store(new IssueHistory
                {
					DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId),
                    IssueId = issue.Id,
                    NewStatus = request.Status,
                    PreviousStatus = issue.Status,
                    SystemMessage = true,
                    UserId = request.CurrentUser.Id,
                    Type = HistoryItemType.StatusUpdated,
                    ApplicationId = issue.ApplicationId,
                });
            }

			issue.Status = request.Status;
	        issue.LastModifiedUtc = DateTime.UtcNow;

			Session.AddCommitAction(new RaiseIssueModifiedEvent(issue));

            return new SetIssueStatusResponse
            {
                Status = SetIssueStatusStatus.Ok
            };
        }
    }

    public interface ISetIssueStatusCommand : ICommand<SetIssueStatusRequest, SetIssueStatusResponse>
    { }

    public class SetIssueStatusResponse
    {
        public SetIssueStatusStatus Status { get; set; }
    }

    public class SetIssueStatusRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public IssueStatus Status { get; set; }
    }

    public enum SetIssueStatusStatus
    {
        Ok,
        IssueNotFound
    }
}
