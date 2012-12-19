using System;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Organisations;
using CodeTrip.Core.Extensions;
using Errordite.Core.Session;
using Raven.Abstractions.Data;

namespace Errordite.Core.Issues.Commands
{
    public class PurgeIssueCommand : SessionAccessBase, IPurgeIssueCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly IDeleteErrorsCommand _deleteErrorsCommand;

        public PurgeIssueCommand(IDeleteErrorsCommand deleteErrorsCommand, IAuthorisationManager authorisationManager)
        {
            _deleteErrorsCommand = deleteErrorsCommand;
            _authorisationManager = authorisationManager;
        }

        public PurgeIssueResponse Invoke(PurgeIssueRequest request)
        {
			Trace("Starting...");
			TraceObject(request);

			var issue = Load<Issue>(Issue.GetId(request.IssueId));

			if (issue == null)
				return new PurgeIssueResponse();

			_authorisationManager.Authorise(issue, request.CurrentUser);

			//delete the issues errors
			Session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.Errors, new IndexQuery
			{
				Query = "IssueId:{0}".FormatWith(issue.Id)
			});

			//delete any daily issue count docs
			Session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.IssueDailyCount, new IndexQuery
			{
				Query = "IssueId:{0}".FormatWith(issue.Id)
			});

			Session.Raven.Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(issue.FriendlyId)).Initialise();

			issue.History.Add(new IssueHistory
			{
				DateAddedUtc = DateTime.UtcNow,
				UserId = request.CurrentUser.Id,
				Type = HistoryItemType.ErrorsPurged,
			});

			issue.ErrorCount = 0;
			issue.LimitStatus = ErrorLimitStatus.Ok;

			return new PurgeIssueResponse();
		}
	}

    public interface IPurgeIssueCommand : ICommand<PurgeIssueRequest, PurgeIssueResponse>
    { }

    public class PurgeIssueResponse
    {}

    public class PurgeIssueRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
    }
}
