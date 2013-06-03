using System;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Errordite.Core.Users.Queries;
using Errordite.Core.Extensions;

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
                    Organisation = request.CurrentUser.ActiveOrganisation
				});
			}

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

            if (issue.UserId != request.AssignedUserId)
            {
                Store(new IssueHistory
                {
					DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId),
                    IssueId = issue.Id,
                    SystemMessage = true,
                    UserId = request.CurrentUser.Id,
                    AssignedToUserId = request.AssignedUserId,
					Type = HistoryItemType.AssignedUserChanged,
					ApplicationId = issue.ApplicationId
                });
            }

			issue.Status = request.Status;
			issue.UserId = request.AssignedUserId;
			issue.Name = request.Name;
			issue.NotifyFrequency = request.NotifyFrequency;
			issue.Reference = request.Reference;
			issue.LastModifiedUtc = DateTime.UtcNow;

            if (request.Comment.IsNotNullOrEmpty())
            {
				Store(new IssueHistory
				{
					DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId),
					IssueId = issue.Id,
					UserId = request.CurrentUser.Id,
					AssignedToUserId = request.AssignedUserId,
					Type = HistoryItemType.Comment,
					Comment = request.Comment,
					ApplicationId = issue.ApplicationId
				});
            }

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
        public string NotifyFrequency { get; set; }
        public IssueStatus Status { get; set; }
        public string Reference { get; set; }
        public string Comment { get; set; }
    }

    public enum UpdateIssueDetailsStatus
    {
        Ok,
        IssueNotFound
    }
}
