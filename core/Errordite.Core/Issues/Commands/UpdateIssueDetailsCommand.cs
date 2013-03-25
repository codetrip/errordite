using System;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;

namespace Errordite.Core.Issues.Commands
{
    public class UpdateIssueDetailsCommand : SessionAccessBase, IUpdateIssueDetailsCommand
    {
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly IAuthorisationManager _authorisationManager;
        private readonly IGetUserQuery _getUserQuery;

        public UpdateIssueDetailsCommand(IAuthorisationManager authorisationManager, 
            ISendNotificationCommand sendNotificationCommand, 
            IGetUserQuery getUserQuery)
        {
            _authorisationManager = authorisationManager;
            _sendNotificationCommand = sendNotificationCommand;
            _getUserQuery = getUserQuery;
        }

        public UpdateIssueDetailsResponse Invoke(UpdateIssueDetailsRequest request)
        {
            Trace("Starting...");

            var issue = Load<Issue>(Issue.GetId(request.IssueId));

            if(issue == null)
            {
                return new UpdateIssueDetailsResponse
                {
                    Status = UpdateIssueDetailsStatus.IssueNotFound
                };
            }

            _authorisationManager.Authorise(issue, request.CurrentUser);

			//if we are assigning this issue to a new user, notify them
			if (issue.UserId != request.AssignedUserId && request.AssignedUserId != request.CurrentUser.Id)
			{
				var user = _getUserQuery.Invoke(new GetUserRequest
				{
					UserId = request.AssignedUserId,
					OrganisationId = issue.OrganisationId
				}).User;

				_sendNotificationCommand.Invoke(new SendNotificationRequest
				{
					EmailInfo = new IssueAssignedToUserEmailInfo
					{
						To = user.Email,
						IssueId = issue.Id,
						IssueName = request.Name
					},
					OrganisationId = issue.OrganisationId,
				});
			}

            if (issue.Status != request.Status)
            {
                Store(new IssueHistory
                {
                    DateAddedUtc = DateTime.UtcNow,
                    IssueId = issue.Id,
                    NewStatus = request.Status,
                    PreviousStatus = issue.Status,
                    SystemMessage = true,
                    UserId = request.CurrentUser.Id,
                    Type = HistoryItemType.StatusUpdated,
                    ApplicationId = issue.ApplicationId,
                });
            }

            if (issue.UserId != request.AssignedUserId)
            {
                Store(new IssueHistory
                {
                    DateAddedUtc = DateTime.UtcNow,
                    IssueId = issue.Id,
                    SystemMessage = true,
                    UserId = request.CurrentUser.Id,
                    AssignedToUserId = request.AssignedUserId,
                    Type = HistoryItemType.AssignedUserChanged
                });
            }

			issue.Status = request.Status;
			issue.UserId = request.AssignedUserId;
			issue.Name = request.Name;
			issue.AlwaysNotify = request.AlwaysNotify;
			issue.Reference = request.Reference;

			Session.AddCommitAction(new RaiseIssueModifiedEvent(issue));

            return new UpdateIssueDetailsResponse
            {
                Status = UpdateIssueDetailsStatus.Ok
            };
        }
    }

    public interface IUpdateIssueDetailsCommand : ICommand<UpdateIssueDetailsRequest, UpdateIssueDetailsResponse>
    { }

    public class UpdateIssueDetailsResponse
    {
        public UpdateIssueDetailsStatus Status { get; set; }
    }

    public class UpdateIssueDetailsRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public string Name { get; set; }
        public string AssignedUserId { get; set; }
        public bool AlwaysNotify { get; set; }
        public IssueStatus Status { get; set; }
        public string Reference { get; set; }
    }

    public enum UpdateIssueDetailsStatus
    {
        Ok,
        IssueNotFound
    }
}
