﻿using System;
using System.Collections.Generic;
using Errordite.Core.Dynamic;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Matching;
using Errordite.Core.Messaging;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Extensions;
using Errordite.Core.Session.Actions;

namespace Errordite.Core.Issues.Commands
{
    public class AdjustRulesCommand : SessionAccessBase, IAdjustRulesCommand
    {
        private readonly IGetErrorsThatDoNotMatchNewRulesQuery _getErrorsThatDoNotMatchNewRulesQuery;
        private readonly IMoveErrorsToNewIssueCommand _moveErrorsToNewIssueCommand;
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public AdjustRulesCommand(IGetErrorsThatDoNotMatchNewRulesQuery getErrorsThatDoNotMatchNewRulesQuery, 
            IMoveErrorsToNewIssueCommand moveErrorsToNewIssueCommand, 
            IAuthorisationManager authorisationManager,
            ErrorditeConfiguration configuration)
        {
            _getErrorsThatDoNotMatchNewRulesQuery = getErrorsThatDoNotMatchNewRulesQuery;
            _moveErrorsToNewIssueCommand = moveErrorsToNewIssueCommand;
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public AdjustRulesResponse Invoke(AdjustRulesRequest request)
        {
            Trace("Starting...");

            var currentIssue = Session.Raven.Load<Issue>(Issue.GetId(request.IssueId));

            AdjustRulesResponse response;
            if (!ValidateCommand(currentIssue, request.Rules, out response))
            {
                return response;
            }

            _authorisationManager.Authorise(currentIssue, request.CurrentUser);

            //craete the new temp issue
            var tempIssue = CreateTempIssue(currentIssue, request);

            var currentDbIssue = currentIssue;

            if (!request.WhatIf)
            {
                //storing at this point makes sure we get an Id for the isssue
                Store(tempIssue);
            }
            else
            {
                //if we're just doing a whatif we don't want to actually change the issue in the db
                //note the properties on the new "currentissue" will still presumably be proxies 
                //so need to be careful not to change anything on them...
                currentIssue = new Issue();
                PropertyMapper.Map(currentDbIssue, currentIssue);
            }

            //and update the existing issue
            UpdateCurrentIssue(currentIssue, request);

            Trace("Starting to determine non matching errors , Current Error Count:={0}, Temp Issue Error Count:={1}...", currentIssue.ErrorCount, tempIssue.ErrorCount);
            var nonMatchingErrorsResponse = _getErrorsThatDoNotMatchNewRulesQuery.Invoke(new GetErrorsThatDoNotMatchNewRulesRequest
            {
                IssueWithModifiedRules = currentIssue,
                IssueWithOldRules = tempIssue
            });
            Trace("Completed determining non matching errors, temp issue error count:={0}, remaining errors:={1}...", tempIssue.ErrorCount, nonMatchingErrorsResponse.Matches);

            if (!request.WhatIf)
            {
                if (nonMatchingErrorsResponse.NonMatches.Count > 0)
                {
                    var dateTimeOffset = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId);

                    //if errors on the original issue did not match the new rules, store the temp issue and move the non matching errors to it
                    Session.AddCommitAction(new RaiseIssueCreatedEvent(tempIssue));

                    //move all these errors to the new issue in a batch
                    _moveErrorsToNewIssueCommand.Invoke(new MoveErrorsToNewIssueRequest
                    {
                        Errors = nonMatchingErrorsResponse.NonMatches,
                        IssueId = tempIssue.Id,
                        CurrentUser = request.CurrentUser
                    });

                    //re-sync the error counts only if we have moved errors (do it for both issues)
                    Session.AddCommitAction(new SendMessageCommitAction(new SyncIssueErrorCountsMessage
                    {
                        IssueId = currentIssue.Id,
                        OrganisationId = request.CurrentUser.OrganisationId,
                        TriggerEventUtc = DateTime.UtcNow,
					}, _configuration.GetEventsQueueAddress(request.CurrentUser.ActiveOrganisation.RavenInstance.FriendlyId)));

					Session.AddCommitAction(new SendMessageCommitAction(new SyncIssueErrorCountsMessage
					{
						IssueId = tempIssue.Id,
						OrganisationId = request.CurrentUser.OrganisationId,
						TriggerEventUtc = DateTime.UtcNow,
					}, _configuration.GetEventsQueueAddress(request.CurrentUser.ActiveOrganisation.RavenInstance.FriendlyId)));

                    Store(new IssueHistory
                    {
                        DateAddedUtc = dateTimeOffset,
                        Type = HistoryItemType.RulesAdjustedCreatedNewIssue,
                        SpawnedIssueId = tempIssue.Id,
                        UserId = request.CurrentUser.Id,
                        IssueId = currentIssue.Id,
						ApplicationId = currentIssue.ApplicationId,
						SystemMessage = true
                    });

                    Store(new IssueHistory
                    {
                        DateAddedUtc = dateTimeOffset,
                        Type = HistoryItemType.CreatedByRuleAdjustment,
                        SpawningIssueId = currentIssue.Id,
                        UserId = request.CurrentUser.Id,
                        IssueId = tempIssue.Id,
						ApplicationId = tempIssue.ApplicationId,
						SystemMessage = true
                    });
                }
                else
                {
                    Store(new IssueHistory
                    {
                        DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId),
                        Type = HistoryItemType.RulesAdjustedNoNewIssue,
                        UserId = request.CurrentUser.Id,
                        IssueId = currentIssue.Id,
                        ApplicationId = currentIssue.ApplicationId,
						SystemMessage = true
                    });

                    Delete(tempIssue);
                }

                Session.AddCommitAction(new RaiseIssueModifiedEvent(currentIssue));
            }

            return new AdjustRulesResponse
            {
                Status = AdjustRulesStatus.Ok,
                IssueId = currentIssue.FriendlyId,
                ErrorsMatched = nonMatchingErrorsResponse.Matches.Count,
                ErrorsNotMatched = nonMatchingErrorsResponse.NonMatches.Count,
                UnmatchedIssueId = tempIssue.FriendlyId,
            };
        }

        private void UpdateCurrentIssue(Issue currentIssue, AdjustRulesRequest request)
        {
            if (currentIssue.Status == IssueStatus.Unacknowledged)
            {
                currentIssue.Status = IssueStatus.Acknowledged;
            }

            currentIssue.Name = request.OriginalIssueName;
            currentIssue.Rules = request.Rules;
            currentIssue.LastRuleAdjustmentUtc = DateTime.UtcNow;
        }
        
        private Issue CreateTempIssue(Issue currentIssue, AdjustRulesRequest request)
        {
            var dateTimeOffset = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId);

            return new Issue
            {
                ApplicationId = currentIssue.ApplicationId,
                CreatedOnUtc = dateTimeOffset,
                ErrorCount = 0,
                LastErrorUtc = currentIssue.LastErrorUtc,
                LastModifiedUtc = dateTimeOffset,
                Name = request.NewIssueName,
                Status = IssueStatus.Unacknowledged, //GT: this used to be dependent on state of existing issue, but I think that's wrong.  The idea is that this issue contains errors we have not yet thought about hence it needs to start at the beginning.
                Rules = currentIssue.Rules,
                UserId = currentIssue.UserId, //GT: by the same token as status, not 100% sure what this should be - will have to suck it and see
                OrganisationId = currentIssue.OrganisationId,
            };
        }

        private bool ValidateCommand(Issue currentIssue, List<IMatchRule> rules, out AdjustRulesResponse response)
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
        public List<IMatchRule> Rules { get; set; }
        public bool WhatIf { get; set; }
    }

    public enum AdjustRulesStatus
    {
        Ok,
        IssueNotFound,
        RulesNotChanged,
    }
}
