using System;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Extensions;

namespace Errordite.Core.Issues.Commands
{
    public class AddCommentCommand : SessionAccessBase, IAddCommentCommand
    {
        public AddCommentResponse Invoke(AddCommentRequest request)
        {
            Trace("Starting...");

            if (request.Comment.IsNotNullOrEmpty())
			{
                Store(new IssueHistory
                {
                    DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.Organisation.TimezoneId),
                    Comment = request.Comment,
                    UserId = request.CurrentUser.Id,
                    Type = HistoryItemType.Comment,
                    IssueId = Issue.GetId(request.IssueId)
                });
			}

            return new AddCommentResponse
            {
                Status = AddCommentStatus.Ok
            };
        }
    }

    public interface IAddCommentCommand : ICommand<AddCommentRequest, AddCommentResponse>
    { }

    public class AddCommentResponse
    {
        public AddCommentStatus Status { get; set; }
    }

    public class AddCommentRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public string Comment { get; set; }
    }

    public enum AddCommentStatus
    {
        Ok,
        IssueNotFound
    }
}
