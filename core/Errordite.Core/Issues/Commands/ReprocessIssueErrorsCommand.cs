using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Reception.Commands;
using CodeTrip.Core.Extensions;
using System.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Commands
{
    public class ReprocessIssueErrorsCommand : SessionAccessBase, IReprocessIssueErrorsCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IReceiveErrorCommand _receiveErrorCommand;

        public ReprocessIssueErrorsCommand(IAuthorisationManager authorisationManager, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            ErrorditeConfiguration configuration,
            IReceiveErrorCommand receiveErrorCommand)
        {
            _authorisationManager = authorisationManager;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _configuration = configuration;
            _receiveErrorCommand = receiveErrorCommand;
        }

        public ReprocessIssueErrorsResponse Invoke(ReprocessIssueErrorsRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            var issue = Load<Issue>(Issue.GetId(request.IssueId));

            if(issue != null)
            {
                try
                {
                    _authorisationManager.Authorise(issue, request.CurrentUser);
                }
                catch (ErrorditeAuthorisationException)
                {
                    return new ReprocessIssueErrorsResponse
                    {
                        Status = ReprocessIssueErrorsStatus.NotAuthorised
                    };
                }

                var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
                {
                    ApplicationId = issue.ApplicationId,
                    IssueId = issue.Id,
                    OrganisationId = issue.OrganisationId,
                    Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
                }).Errors;

                var responses = errors.Items.Select(error => _receiveErrorCommand.Invoke(new ReceiveErrorRequest
                {
                    ApplicationId = issue.ApplicationId, 
                    Error = error, 
                    ExistingIssueId = issue.Id, 
                    OrganisationId = issue.OrganisationId
                })).ToList();

                var response = new ReprocessIssueErrorsResponse
                {
                    AttachedIssueIds = responses.GroupBy(r => r.IssueId).ToDictionary(g => g.Key, g => g.Count()),
                    Status = ReprocessIssueErrorsStatus.Ok
                };

                issue.History.Add(new IssueHistory
                {
                    DateAddedUtc = DateTime.UtcNow,
                    UserId = request.CurrentUser.Id,
                    Type = HistoryItemType.ErrorsReprocessed,
                    ReprocessingResult = response.AttachedIssueIds,
                });

                if (response.AttachedIssueIds.Any(i => i.Key == issue.Id))
                {
                    var issueCount = response.AttachedIssueIds.First(i => i.Key == issue.Id);
                    issue.LimitStatus = issueCount.Value >= _configuration.IssueErrorLimit
                        ? ErrorLimitStatus.Exceeded
                        : ErrorLimitStatus.Ok;
                    issue.ErrorCount = issueCount.Value;
                }
                else
                {
                    issue.LimitStatus = ErrorLimitStatus.Ok;
                    issue.ErrorCount = 0;
                }

                Session.SynchroniseIndexes<Issues_Search, Errors_Search>();
                return response;
            }

            return new ReprocessIssueErrorsResponse
            {
                Status = ReprocessIssueErrorsStatus.IssueNotFound
            };
        }
    }

    public interface IReprocessIssueErrorsCommand : ICommand<ReprocessIssueErrorsRequest, ReprocessIssueErrorsResponse>
    { }

    public class ReprocessIssueErrorsResponse
    {
        public ReprocessIssueErrorsStatus Status { get; set; }
        public IDictionary<string, int> AttachedIssueIds { get; set; }

        public MvcHtmlString GetMessage(string issueId)
        {
            if (Status == ReprocessIssueErrorsStatus.IssueNotFound)
                return new MvcHtmlString("Failed to load the requested issue for reprocessing");

            string message;
            int attachedToThis;
            if (AttachedIssueIds.TryGetValue(issueId, out attachedToThis))
            {
                if (AttachedIssueIds.Count == 1)
                {
                    message = "All errors remain attached to this issue.";
                }
                else
                {
                    var otherAttachedIssueIds = AttachedIssueIds.Keys.Where(k => k != issueId);
                    message = "{0} error{1} remain attached to this issue. The rest became attached to issue{2} {3}"
                        .FormatWith(attachedToThis, attachedToThis == 1 ? "" : "s", otherAttachedIssueIds.Count() == 1 ? "" : "s",
                                    otherAttachedIssueIds
                                        .StringConcat(k => " {0}:{1}".FormatWith(GetIssueLink(k), AttachedIssueIds[k])));
                }
            }
            else if (AttachedIssueIds.Count == 1)
            {
                message = "All errors became attached to issue {0}".FormatWith(GetIssueLink(AttachedIssueIds.First().Key));
            }
            else
            {
                message = "All errors became attached to other issues: {0}".FormatWith(AttachedIssueIds.StringConcat(k => " {0}:{1}".FormatWith(GetIssueLink(k.Key), k.Value)));
            }

            return new MvcHtmlString("Errors re-processed successfully. " + message);
        }

        private string GetIssueLink(string issueId)
        {
            return "<a href='/issue/{0}'>{0}</a>".FormatWith(IdHelper.GetFriendlyId(issueId));
        }
    }

    public enum ReprocessIssueErrorsStatus
    {
        Ok,
        IssueNotFound,
        NotAuthorised
    }

    public class ReprocessIssueErrorsRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public string OrganisationId { get; set; }
    }
}
