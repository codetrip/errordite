using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Matching;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Raven.Abstractions.Data;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Issues.Commands
{
    public class AdjustRulesCommand : SessionAccessBase, IAdjustRulesCommand
    {
        private readonly IGetIssueWithMatchingRulesQuery _getIssueWithMatchingRulesQuery;
        private readonly IGetErrorsThatDoNotMatchNewRulesQuery _getErrorsThatDoNotMatchNewRulesQuery;
        private readonly IMoveErrorsToNewIssueCommand _moveErrorsToNewIssueCommand;
        private readonly IMergeIssuesCommand _mergeIssuesCommand;
        private readonly IAuthorisationManager _authorisationManager;

        public AdjustRulesCommand(IGetIssueWithMatchingRulesQuery getIssueWithMatchingRulesQuery, 
            IGetErrorsThatDoNotMatchNewRulesQuery getErrorsThatDoNotMatchNewRulesQuery, 
            IMoveErrorsToNewIssueCommand moveErrorsToNewIssueCommand, 
            IMergeIssuesCommand mergeIssuesCommand, 
            IAuthorisationManager authorisationManager)
        {
            _getIssueWithMatchingRulesQuery = getIssueWithMatchingRulesQuery;
            _getErrorsThatDoNotMatchNewRulesQuery = getErrorsThatDoNotMatchNewRulesQuery;
            _moveErrorsToNewIssueCommand = moveErrorsToNewIssueCommand;
            _mergeIssuesCommand = mergeIssuesCommand;
            _authorisationManager = authorisationManager;
        }

        public AdjustRulesResponse Invoke(AdjustRulesRequest request)
        {
            Trace("Starting...");

            var currentIssue = Session.Raven.Load<Issue>(Issue.GetId(request.IssueId));
            var currentStatus = currentIssue.Status;

            AdjustRulesResponse response;
            if (!ValidateCommand(currentIssue, request.Rules, out response))
            {
                return response;
            }

            _authorisationManager.Authorise(currentIssue, request.CurrentUser);

            //craete the new temp issue
            var tempIssue = CreateTempIssue(currentIssue, request);

            //makes sure we get an Id for the isssue
            Store(tempIssue);

            //and update the existing issue
            UpdateCurrentIssue(currentIssue, tempIssue, request);

            Trace("Starting to determining non matching errors , Current Error Count:={0}, Temp Issue Error Count:={1}...", currentIssue.ErrorCount, tempIssue.ErrorCount);
            var nonMatchingErrorsResponse = _getErrorsThatDoNotMatchNewRulesQuery.Invoke(new GetErrorsThatDoNotMatchNewRulesRequest
            {
                IssueWithModifiedRules = currentIssue,
                IssueWithOldRules = tempIssue
            });
            Trace("Completed determining non matching errors, temp issue error count:={0}, remaining errors:={1}...", tempIssue.ErrorCount, nonMatchingErrorsResponse.MatchCount);

            if (nonMatchingErrorsResponse.Errors.Count > 0)
            {
                //if errors on the original issue did not match the new rules, store the temp issue and move the non matching errors to it
                Store(tempIssue);
                Session.AddCommitAction(new RaiseIssueCreatedEvent(tempIssue));

                //also we need to clear the core errors for the original issue as we no longer know if these should match the new rules
                Session.Raven.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.UnloggedErrors, new IndexQuery
                {
                    Query = "IssueId:{0}".FormatWith(currentIssue.Id)
                }, true);

                //move all these errors to the new issue in a batch
                _moveErrorsToNewIssueCommand.Invoke(new MoveErrorsToNewIssueRequest
                {
                    Errors = nonMatchingErrorsResponse.Errors,
                    IssueId = tempIssue.Id
                });
            }
            else
            {
                Delete(tempIssue);
            }

            //this bit is checking to see if there are any other issues for this application which have the same rule set
            //as the issue we just created, if there is we either auto merge the issues (if the matching issue is unacknowledged)
            //or return a status which will trigger an manual merge by the user.
            var matchingIssue = GetMatchingIssue(currentIssue);
            string retainedIssueId = null;

            if(matchingIssue != null)
            {
                if (matchingIssue.Status == IssueStatus.Unacknowledged || currentStatus == IssueStatus.Unacknowledged)
                {
                    retainedIssueId = currentStatus == IssueStatus.Unacknowledged
                        ? matchingIssue.FriendlyId
                        : currentIssue.FriendlyId;

                    //auto merge with currentIssue
                    _mergeIssuesCommand.Invoke(new MergeIssuesRequest
                    {
                        MergeToIssueId = currentStatus == IssueStatus.Unacknowledged ? matchingIssue.Id : currentIssue.Id,
                        MergeFromIssueId = currentStatus == IssueStatus.Unacknowledged ? currentIssue.Id : matchingIssue.Id,
                        CurrentUser = request.CurrentUser
                    });
                }
                else
                {
                    //tell the user whats up and let them choose how to merge
                    return new AdjustRulesResponse
                    {
                        IssueId = currentIssue.FriendlyId,
                        Status = AdjustRulesStatus.RulesMatchedOtherIssue,
                        MatchingIssueId = matchingIssue.FriendlyId
                    };
                }
            }

            Session.AddCommitAction(new RaiseIssueModifiedEvent(currentIssue));

            return new AdjustRulesResponse
            {
                Status = retainedIssueId == null ? AdjustRulesStatus.Ok : AdjustRulesStatus.AutoMergedWithOtherIssue,
                IssueId = retainedIssueId ?? currentIssue.FriendlyId,
                ErrorsMatched = nonMatchingErrorsResponse.MatchCount,
                ErrorsNotMatched = nonMatchingErrorsResponse.Errors.Count,
                UnmatchedIssueId = tempIssue.FriendlyId,
            };
        }

        private void UpdateCurrentIssue(Issue currentIssue, Issue tempIssue, AdjustRulesRequest request)
        {
            if (currentIssue.Status == IssueStatus.Unacknowledged)
            {
                currentIssue.Status = IssueStatus.Acknowledged;

                Session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Errors,
                    new IndexQuery
                    {
                        Query = "IssueId:{0} AND Classified:false".FormatWith(currentIssue.Id)
                    },
                    new[]
                    {
                        new PatchRequest
                        {
                            Name = "Classified",
                            Type = PatchCommandType.Set,
                            Value = true
                        }
                    }, true);
            }

            currentIssue.Name = request.OriginalIssueName;
            currentIssue.Rules = request.Rules;
            currentIssue.History.Add(new IssueHistory
            {
                DateAddedUtc = DateTime.UtcNow,
                Message = Resources.CoreResources.HistoryRulesAdjusted.FormatWith(request.CurrentUser.FullName, request.CurrentUser.Email, tempIssue.FriendlyId),
                SystemMessage = true,
            });
        }
        
        private Issue CreateTempIssue(Issue currentIssue, AdjustRulesRequest request)
        {
            return new Issue
            {
                ApplicationId = currentIssue.ApplicationId,
                CreatedOnUtc = DateTime.UtcNow,
                ErrorCount = 0,
                LastErrorUtc = currentIssue.LastErrorUtc,
                LastModifiedUtc = DateTime.UtcNow,
                Name = request.NewIssueName,
                Status = IssueStatus.Unacknowledged, //GT: this used to be dependent on state of existing issue, but I think that's wrong.  The idea is that this issue contains errors we have not yet thought about hence it needs to start at the beginning.
                Rules = currentIssue.Rules,
                UserId = currentIssue.UserId, //GT: by the same token as status, not 100% sure what this should be - will have to suck it and see
                OrganisationId = currentIssue.OrganisationId,
                MatchPriority = currentIssue.MatchPriority,
                History = new List<IssueHistory>
                {
                    new IssueHistory
                    {
                        DateAddedUtc = DateTime.UtcNow,
                        Message = Resources.CoreResources.HistoryCreatedByRulesAdjustment.FormatWith(currentIssue.FriendlyId, request.CurrentUser.FullName, request.CurrentUser.Email),
                        SystemMessage = true,
                    }
                }
            };
        }

        private Issue GetMatchingIssue(Issue issue)
        {
            var matchingIssue = _getIssueWithMatchingRulesQuery.Invoke(new GetIssueWithMatchingRulesRequest
            {
                IssueToMatch = issue,
            });

            if (matchingIssue.Issue != null)
            {
                Trace("Rules exist on another issue:={0}, returning...", matchingIssue.Issue.Id);
                return matchingIssue.Issue;
            }

            return null;
        }

        private bool ValidateCommand(Issue currentIssue, IList<IMatchRule> rules, out AdjustRulesResponse response)
        {
            if (currentIssue == null)
            {
                response = new AdjustRulesResponse
                {
                    Status = AdjustRulesStatus.IssueNotFound
                };
                return false;
            }

            Trace("Located existing issue...");

            if (currentIssue.RulesEqual(rules))
            {
                Trace("Rules have not changed, returning...");

                response = new AdjustRulesResponse
                {
                    Status = AdjustRulesStatus.RulesNotChanged,
                    IssueId = currentIssue.FriendlyId
                };
                return false;
            }

            response = null;
            return true;
        }
    }

    public interface IAdjustRulesCommand : ICommand<AdjustRulesRequest, AdjustRulesResponse>
    { }

    public class AdjustRulesResponse
    {
        public AdjustRulesStatus Status { get; set; }
        public string IssueId { get; set; }
        public string MatchingIssueId { get; set; }
        public int ErrorsMatched { get; set; }
        public int ErrorsNotMatched { get; set; }
        public string UnmatchedIssueId { get; set; }
    }

    public class AdjustRulesRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public string ApplicationId { get; set; }
        public string NewIssueName { get; set; }
        public string OriginalIssueName { get; set; }
        public MatchPriority NewPriority { get; set; }
        public MatchPriority OriginalPriority { get; set; }
        public IList<IMatchRule> Rules { get; set; }
    }

    public enum AdjustRulesStatus
    {
        Ok,
        RulesMatchedOtherIssue,
        IssueNotFound,
        RulesNotChanged,
        AutoMergedWithOtherIssue
    }
}
