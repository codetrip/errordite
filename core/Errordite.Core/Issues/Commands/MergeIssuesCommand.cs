using System;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Resources;
using CodeTrip.Core.Extensions;
using Raven.Abstractions.Data;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Commands
{
    public class MergeIssuesCommand : SessionAccessBase, IMergeIssuesCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public MergeIssuesCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
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

            //move all errors fron the MergeFromIssue to the MergeToIssue
            Session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Errors,
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
            }, true);

            //also move all core errors fron the MergeFromIssue to the MergeToIssue
            Session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.UnloggedErrors,
                new IndexQuery
                {
                    Query = "IssueId:{0}".FormatWith(mergeFromIssue.Id)
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "IssueId",
                        Type = PatchCommandType.Set,
                        Value = mergeToIssue.Id
                    }   
            }, true);

            mergeToIssue.History.Add(new IssueHistory
            {
                DateAddedUtc = DateTime.UtcNow,
                SpawningIssueId = mergeFromIssue.Id,
                //Message = CoreResources.HIstoryIssueMerged.FormatWith(mergeFromIssue.FriendlyId, mergeFromIssue.Name),
                SystemMessage = true,
                Type = HistoryItemType.MergedTo,
            });

            Delete(mergeFromIssue);

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
