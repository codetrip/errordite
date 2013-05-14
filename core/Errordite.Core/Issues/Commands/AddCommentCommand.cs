using System;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Organisations;
using Errordite.Core.Session;

namespace Errordite.Core.Issues.Commands
{
	public class AddCommentCommand : SessionAccessBase, IAddCommentCommand
	{
		private readonly IAuthorisationManager _authorisationManager;

		public AddCommentCommand(IAuthorisationManager authorisationManager)
		{
			_authorisationManager = authorisationManager;
		}

		public AddCommentResponse Invoke(AddCommentRequest request)
		{
			Trace("Starting...");

			var issue = Load<Issue>(Issue.GetId(request.IssueId));

			if (issue == null)
			{
				return new AddCommentResponse
				{
					Status = AddCommentStatus.IssueNotFound
				};
			}

			_authorisationManager.Authorise(issue, request.CurrentUser);

			if (request.Comment.IsNotNullOrEmpty())
			{
				Store(new IssueHistory
				{
					DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId),
					IssueId = issue.Id,
					UserId = request.CurrentUser.Id,
					Type = HistoryItemType.Comment,
					Comment = request.Comment,
					ApplicationId = issue.ApplicationId
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
