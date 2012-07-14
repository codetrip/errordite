using System;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;
using Raven.Abstractions.Data;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

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

            if (issue.Status == IssueStatus.Unacknowledged && request.Status != IssueStatus.Unacknowledged)
            {
                Session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Errors,
                    new IndexQuery
                    {
                        Query = "IssueId:{0} AND Classified:false".FormatWith(issue.Id)
                    },
                    new[]
                    {
                        new PatchRequest
                        {
                            Name = "Classified",
                            Type = PatchCommandType.Set,
                            Value = true
                        }
                    });
            }

            //if we are assigning this issue to a new user, notify them
            if (issue.UserId != request.AssignedUserId)
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

            issue.Status = request.Status;
            issue.UserId = request.AssignedUserId;
            issue.Name = request.Name;
            issue.AlwaysNotify = request.AlwaysNotify;
            //issue.MatchPriority = request.Priority;
            issue.Reference = request.Reference;

            string message = Resources.CoreResources.HistoryIssueUpdated.FormatWith(request.CurrentUser.FullName,
                request.CurrentUser.Email,
                request.Status, 
                request.Name,
                request.AlwaysNotify);

            if (request.Comment.IsNotNullOrEmpty())
                message = "{0}<br /><strong>Comment: </strong><i>{1}</i>".FormatWith(message, request.Comment);

            issue.History.Add(new IssueHistory
            {
                Reference = request.Reference,
                DateAddedUtc = DateTime.UtcNow,
                Message = message,
                UserId = request.CurrentUser.Id,
            });

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
        public string Comment { get; set; }
        public string AssignedUserId { get; set; }
        public bool AlwaysNotify { get; set; }
        public IssueStatus Status { get; set; }
        public MatchPriority Priority { get; set; }
        public string Reference { get; set; }
    }

    public enum UpdateIssueDetailsStatus
    {
        Ok,
        IssueNotFound
    }
}
