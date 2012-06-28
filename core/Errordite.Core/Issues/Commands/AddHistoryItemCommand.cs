using System;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;

namespace Errordite.Core.Issues.Commands
{
    public class AddHistoryItemCommand : SessionAccessBase, IAddHistoryItemCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public AddHistoryItemCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public AddHistoryItemResponse Invoke(AddHistoryItemRequest request)
        {
            Trace("Starting...");

            var issue = Load<Issue>(Issue.GetId(request.IssueId));

            if (issue == null)
            {
                return new AddHistoryItemResponse
                {
                    Status = AddHistoryItemStatus.IssueNotFound
                };
            }

            _authorisationManager.Authorise(issue, request.CurrentUser);

            issue.History.Add(new IssueHistory
            {
                UserId = request.CurrentUser.Id,
                DateAddedUtc = DateTime.UtcNow,
                Message = request.Message,
                Changeset = request.Changeset
            });

            return new AddHistoryItemResponse
            {
                Status = AddHistoryItemStatus.Ok
            };
        }
    }

    public interface IAddHistoryItemCommand : ICommand<AddHistoryItemRequest, AddHistoryItemResponse>
    { }

    public class AddHistoryItemResponse
    {
        public AddHistoryItemStatus Status { get; set; }
    }

    public class AddHistoryItemRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public string Message { get; set; }
        public string Changeset { get; set; }
    }

    public enum AddHistoryItemStatus
    {
        Ok,
        IssueNotFound
    }
}
