using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;
using Raven.Abstractions.Data;

namespace Errordite.Core.Issues.Commands
{
    public class BatchStatusUpdateCommand : SessionAccessBase, IBatchStatusUpdateCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly IGetUserQuery _getUserQuery;

        public BatchStatusUpdateCommand(IAuthorisationManager authorisationManager, IGetUserQuery getUserQuery)
        {
            _authorisationManager = authorisationManager;
            _getUserQuery = getUserQuery;
        }

        public BatchStatusUpdateResponse Invoke(BatchStatusUpdateRequest request)
        {
            Trace("Starting...");

            User assignToUser = null;
            if (request.AssignToUserId != null)
            {
                assignToUser = _getUserQuery.Invoke(new GetUserRequest
                {
                    OrganisationId = request.CurrentUser.OrganisationId,
                    UserId = request.AssignToUserId
                }).User;

                _authorisationManager.Authorise(assignToUser, request.CurrentUser);
            }

            var issuesToUpdate = new List<Issue>();

            foreach(var issueId in request.IssueIds.Select(issueId => issueId.Split('|')[0]))
            {
                var issue = Load<Issue>(Issue.GetId(issueId));

                if (issue != null)
                {
                    _authorisationManager.Authorise(issue, request.CurrentUser);
                    
                    issuesToUpdate.Add(issue);

                    if (issue.Status == IssueStatus.Unacknowledged)
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
                            }, true);
                    }

                    if (request.AssignToUserId != null)
                    {
                        issue.UserId = request.AssignToUserId;
                    }

                    issue.History.Add(new IssueHistory
                    {
                        DateAddedUtc = DateTime.UtcNow,
                        UserId = request.CurrentUser.Id,
                        Message = "{0}{1}{2}".FormatWith(Resources.CoreResources.HistoryIssueStatusUpdated.FormatWith(issue.Status, request.Status, request.CurrentUser.FullName, request.CurrentUser.Email),
                            assignToUser == null ? "" : "{0}Assigned to {1} ({2})".FormatWith(Environment.NewLine, assignToUser.FullName, assignToUser.Email),
                            request.Comment == null ? "" : Environment.NewLine + request.Comment),
                    });

                    issue.Status = request.Status;
                }
            }

            if (issuesToUpdate.Count > 0)
            {
                Session.AddCommitAction(new RaiseIssueModifiedEvent(issuesToUpdate));
            }

            return new BatchStatusUpdateResponse
            {
                Status = BulkStatusUpdateStatus.Ok
            };
        }
    }

    public interface IBatchStatusUpdateCommand : ICommand<BatchStatusUpdateRequest, BatchStatusUpdateResponse>
    { }

    public class BatchStatusUpdateResponse
    {
        public BulkStatusUpdateStatus Status { get; set; }
    }

    public class BatchStatusUpdateRequest : OrganisationRequestBase
    {
        public List<string> IssueIds { get; set; }
        public IssueStatus Status { get; set; }
        public string Comment { get; set; }
        public string AssignToUserId { get; set; }
    }

    public enum BulkStatusUpdateStatus
    {
        Ok
    }
}
