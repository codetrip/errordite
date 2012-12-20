﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;

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

            if (request.AssignToUserId != null)
            {
                var assignToUser = _getUserQuery.Invoke(new GetUserRequest
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

                    if (request.AssignToUserId != null)
                    {
                        issue.UserId = request.AssignToUserId;
                    }

                    issue.History.Add(new IssueHistory
                    {
                        DateAddedUtc = DateTime.UtcNow,
                        UserId = request.CurrentUser.Id,
                        AssignedToUserId = request.AssignToUserId,
                        Comment = request.Comment,
                        PreviousStatus = issue.Status,
                        NewStatus = request.Status,
                        Type = HistoryItemType.BatchStatusUpdate,
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
