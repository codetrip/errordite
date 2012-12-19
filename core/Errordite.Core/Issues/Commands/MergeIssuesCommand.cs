using System;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Messages;
using Errordite.Core.Organisations;
using CodeTrip.Core.Extensions;
using Errordite.Core.Session;
using Raven.Abstractions.Data;

namespace Errordite.Core.Issues.Commands
{
    public class MergeIssuesCommand : SessionAccessBase, IMergeIssuesCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public MergeIssuesCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public MergeIssuesResponse Invoke(MergeIssuesRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            var mergeFromIssue = Load<Issue>(Issue.GetId(request.MergeFromIssueId));
            var mergeToIssue = Load<Issue>(Issue.GetId(request.MergeToIssueId));

            MergeIssuesResponse response;
            if (!ValidateCommand(mergeFromIssue, mergeToIssue, out response))
            {
                return response;
            }

            _authorisationManager.Authorise(mergeFromIssue, request.CurrentUser);
            _authorisationManager.Authorise(mergeToIssue, request.CurrentUser);

            new SynchroniseIndex<Errors_Search>().Execute(Session);

            //move all errors fron the MergeFromIssue to the MergeToIssue
            Session.AddCommitAction(new UpdateByIndexCommitAction(CoreConstants.IndexNames.Errors,
                new IndexQuery
                {
                    Query = "IssueId:{0}".FormatWith(mergeFromIssue.Id)
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "Classified",
                        Type = PatchCommandType.Set,
                        Value = true
                    },
                    new PatchRequest
                    {
                        Name = "IssueId",
                        Type = PatchCommandType.Set,
                        Value = mergeToIssue.Id
                    }
            }, true));

            mergeToIssue.History.Add(new IssueHistory
            {
                DateAddedUtc = DateTime.UtcNow,
                SpawningIssueId = mergeFromIssue.Id,
                SystemMessage = true,
                Type = HistoryItemType.MergedTo,
            });

            Delete(mergeFromIssue);

            //re-sync the error counts
            Session.AddCommitAction(new SendNServiceBusMessage("Sync Issue Error Counts", new SyncIssueErrorCountsMessage
            {
                CurrentUser = request.CurrentUser,
                IssueId = request.MergeToIssueId,
                OrganisationId = request.CurrentUser.OrganisationId
            }, _configuration.EventsQueueName));

            return new MergeIssuesResponse
            {
                Status = MergeIssuesStatus.Ok
            };
        }

        private bool ValidateCommand(Issue mergeFromIssue, Issue mergeToIssue, out MergeIssuesResponse response)
        {
            if (mergeFromIssue == null || mergeToIssue == null)
            {
                response = new MergeIssuesResponse
                {
                    Status = MergeIssuesStatus.IssueNotFound
                };
                return false;
            }
            
            if (mergeFromIssue.RulesHash != mergeToIssue.RulesHash)
            {
                response = new MergeIssuesResponse
                {
                    Status = MergeIssuesStatus.RulesDoNotMatch
                };
                return false;
            }

            response = null;
            return true;
        }
    }

    public interface IMergeIssuesCommand : ICommand<MergeIssuesRequest, MergeIssuesResponse>
    { }

    public class MergeIssuesResponse
    {
        public MergeIssuesStatus Status { get; set; }
    }

    public enum MergeIssuesStatus
    {
        Ok,
        IssueNotFound,
        RulesDoNotMatch
    }

    public class MergeIssuesRequest : OrganisationRequestBase
    {
        public string MergeFromIssueId { get; set; }
        public string MergeToIssueId { get; set; }
        public string MergedIssueName { get; set; }
        public MatchPriority? MergedIssuePriority { get; set; }
    }
}
