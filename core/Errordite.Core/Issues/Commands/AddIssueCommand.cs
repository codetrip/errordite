using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Matching;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Commands
{
    public class AddIssueCommand : SessionAccessBase, IAddIssueCommand
    {
        private readonly IGetIssueWithMatchingRulesQuery _getIssueWithMatchingRulesQuery;

        public AddIssueCommand(IGetIssueWithMatchingRulesQuery getIssueWithMatchingRulesQuery)
        {
            _getIssueWithMatchingRulesQuery = getIssueWithMatchingRulesQuery;
        }

        public AddIssueResponse Invoke(AddIssueRequest request)
        {
            Trace("Starting...");

            var applicationId = Application.GetId(request.ApplicationId);

            var issue = new Issue
            {
                Name = request.Name,
                Rules = request.Rules,
                ApplicationId = applicationId,
                CreatedOnUtc = DateTime.UtcNow,
                LastModifiedUtc = DateTime.UtcNow,
                UserId = User.GetId(request.UserId),
                ErrorCount = 0,
                LastErrorUtc = DateTime.UtcNow,
                OrganisationId = Organisation.GetId(request.CurrentUser.OrganisationId),
                History = new List<IssueHistory>
                {
                    new IssueHistory
                    {
                        DateAddedUtc = DateTime.UtcNow,
                        Message = Resources.CoreResources.HistoryIssueCreatedBy.FormatWith(request.CurrentUser.FullName, request.CurrentUser.Email),
                        UserId = request.CurrentUser.Id
                    }
                },
                MatchPriority = MatchPriority.Low,
            };

            var issuesWithSameRules = _getIssueWithMatchingRulesQuery.Invoke(new GetIssueWithMatchingRulesRequest
            {
                IssueToMatch = issue,
            });

            if (issuesWithSameRules.Issue != null)
            {
                return new AddIssueResponse
                {
                    IssueId = issuesWithSameRules.Issue.FriendlyId,
                    Status = AddIssueStatus.SameRulesExist
                };
            }

            Store(issue);
            Session.AddCommitAction(new RaiseIssueCreatedEvent(issue));

            return new AddIssueResponse
            {
                Status = AddIssueStatus.Ok,
                IssueId = issue.FriendlyId
            };
        }
    }

    public interface IAddIssueCommand : ICommand<AddIssueRequest, AddIssueResponse>
    { }

    public class AddIssueResponse
    {
        public string IssueId { get; set; }
        public AddIssueStatus Status { get; set; }
    }

    public class AddIssueRequest : OrganisationRequestBase
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public string ApplicationId { get; set; }
        public MatchPriority Priority { get; set; }
        public IssueStatus Status { get; set; }
        public IList<IMatchRule> Rules { get; set; }
    }

    public enum AddIssueStatus
    {
        Ok,
        SameRulesExist
    }
}
