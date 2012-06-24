using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Configuration;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Reception.Commands;
using Errordite.Core.Resources;
using CodeTrip.Core.Extensions;
using System.Linq;
using Newtonsoft.Json;

namespace Errordite.Core.Issues.Commands
{
    public class ReceiveIssueErrorsAgainCommand : SessionAccessBase, IReceiveIssueErrorsAgainCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetApplicationQuery _getApplicationQuery;

        public ReceiveIssueErrorsAgainCommand( 
            IAuthorisationManager authorisationManager, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            ErrorditeConfiguration configuration,
            IGetApplicationQuery getApplicationQuery)
        {
            _authorisationManager = authorisationManager;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _configuration = configuration;
            _getApplicationQuery = getApplicationQuery;
        }

        public ReceiveIssueErrorsAgainResponse Invoke(ReceiveIssueErrorsAgainRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            var issue = Load<Issue>(Issue.GetId(request.IssueId));

            if(issue != null)
            {
                _authorisationManager.Authorise(issue, request.CurrentUser);
                
                var application = _getApplicationQuery.Invoke(new GetApplicationRequest
                {
                    CurrentUser = request.CurrentUser,
                    Id = issue.ApplicationId,
                    OrganisationId = issue.OrganisationId,
                }).Application;

                var errors = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
                {
                    ApplicationId = issue.ApplicationId,
                    IssueId = issue.Id,
                    OrganisationId = issue.OrganisationId,
                    Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
                }).Errors;

                var requests = errors.Items.Select(error => new ReceiveErrorRequest
                {
                    ApplicationId = issue.ApplicationId,
                    Error = error,
                    ExistingIssueId = issue.Id,
                    OrganisationId = issue.OrganisationId,
                    Token = application.Token,
                });

                //send the errors to the reception service, wait for the response and extract the responses from the response
                var processErrorsTask = new HttpClient().PostAsJsonAsync("{0}/api/errors".FormatWith(_configuration.ReceptionHttpEndpoint), requests);
                processErrorsTask.Wait();

                var read = processErrorsTask.Result.Content.ReadAsStringAsync();
                read.Wait();

                var responses = JsonConvert.DeserializeObject<IEnumerable<ReceiveErrorResponse>>(read.Result);

                var response = new ReceiveIssueErrorsAgainResponse
                {
                    AttachedIssueIds = responses.GroupBy(r => r.IssueId).ToDictionary(g => g.Key, g => g.Count())
                };
                
                issue.History.Add(new IssueHistory
                {
                    DateAddedUtc = DateTime.UtcNow,
                    Message = CoreResources.HistoryIssueErrorsReceivedAgain.FormatWith(
                        request.CurrentUser.FullName, 
                        request.CurrentUser.Email,
                        response.GetMessage(request.BaseIssueUrl, issue.Id)),
                    UserId = request.CurrentUser.Id,
                });

                issue.ErrorCount = 0;
                issue.LimitStatus = ErrorLimitStatus.Ok;

                Session.SynchroniseIndexes<Issues_Search, Errors_Search>();

                return response;
            }

            return new ReceiveIssueErrorsAgainResponse();
        }
    }

    public interface IReceiveIssueErrorsAgainCommand : ICommand<ReceiveIssueErrorsAgainRequest, ReceiveIssueErrorsAgainResponse>
    { }

    public class ReceiveIssueErrorsAgainResponse
    {
        public IDictionary<string, int> AttachedIssueIds { get; set; }

        public MvcHtmlString GetMessage(string baseIssueUrl, string issueId)
        {
            if (AttachedIssueIds == null)
                return new MvcHtmlString("Failed to locate issue");

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
                                        .StringConcat(k => " {0}:{1}".FormatWith(GetIssueLink(baseIssueUrl, k), AttachedIssueIds[k])));
                }
            }
            else if (AttachedIssueIds.Count == 1)
            {
                message = "All errors became attached to issue {0}".FormatWith(GetIssueLink(baseIssueUrl, AttachedIssueIds.First().Key));
            }
            else
            {
                message = "All errors became attached to other issues: {0}".FormatWith(AttachedIssueIds.StringConcat(k => " {0}:{1}".FormatWith(GetIssueLink(baseIssueUrl, k.Key), k.Value)));
            }

            return new MvcHtmlString("Errors re-processed successfully. " + message);
        }

        private string GetIssueLink(string baseIssueUrl, string issueId)
        {
            return "<a href='{0}'>{1}</a>".FormatWith(baseIssueUrl.FormatWith(issueId), IdHelper.GetFriendlyId(issueId));
        }
    }

    public class ReceiveIssueErrorsAgainRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public string BaseIssueUrl { get; set; }
    }
}
