using System;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Organisations;
using Errordite.Core.Resources;
using CodeTrip.Core.Extensions;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

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

            if(issue != null)
            {
                _authorisationManager.Authorise(issue, request.CurrentUser);

                _deleteErrorsCommand.Invoke(new DeleteErrorsRequest
                {
                    IssueIds = new []{ issue.Id },
                    CurrentUser = request.CurrentUser
                });

                issue.History.Add(new IssueHistory
                {
                    DateAddedUtc = DateTime.UtcNow,
                    //Message = CoreResources.HistoryIssuePurged.FormatWith(request.CurrentUser.FullName, request.CurrentUser.Email),
                    UserId = request.CurrentUser.Id,    
                    Type = HistoryItemType.ErrorsPurged,
                });

                issue.ErrorCount = 0;
                issue.LimitStatus = ErrorLimitStatus.Ok;
            }

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
