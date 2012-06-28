﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Commands;
using Errordite.Core.Extensions;
using Errordite.Core.Issues;
using Errordite.Core.Matching;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;

namespace Errordite.Core.Reception.Commands
{
    public class ReceiveErrorCommand : SessionAccessBase, IReceiveErrorCommand
    {
        private readonly IMatchRuleFactoryFactory _matchRuleFactoryFactory;
        private readonly IReceptionServiceIssueCache _receptionServiceIssueCache;
        private readonly IGetApplicationQuery _getApplicationQuery;
        private readonly IGetApplicationByTokenQuery _getApplicationByTokenQuery;
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IMakeExceededErrorsUnloggedCommand _makeExceededErrorsUnloggedCommand;

        public ReceiveErrorCommand(IMatchRuleFactoryFactory matchRuleFactoryFactory, 
            IGetApplicationQuery getApplicationQuery, 
            ISendNotificationCommand sendNotificationCommand, 
            IReceptionServiceIssueCache receptionServiceIssueCache, 
            ErrorditeConfiguration configuration, 
            IMakeExceededErrorsUnloggedCommand makeExceededErrorsUnloggedCommand, 
            IGetApplicationByTokenQuery getApplicationByTokenQuery)
        {
            _matchRuleFactoryFactory = matchRuleFactoryFactory;
            _getApplicationQuery = getApplicationQuery;
            _sendNotificationCommand = sendNotificationCommand;
            _receptionServiceIssueCache = receptionServiceIssueCache;
            _configuration = configuration;
            _makeExceededErrorsUnloggedCommand = makeExceededErrorsUnloggedCommand;
            _getApplicationByTokenQuery = getApplicationByTokenQuery;
        }

        public ReceiveErrorResponse Invoke(ReceiveErrorRequest request)
        {
            Trace("Starting...");

            var application = GetApplication(request);

            if(application == null)
            {
                Trace("Application could not be found");
                return new ReceiveErrorResponse();
            }

            Trace("ApplicationId:={0}, OrganisationId:={1}", application.Id, application.OrganisationId);

            var error = request.Error;
            var issues = _receptionServiceIssueCache.GetIssues(application.Id);
            var existingIssue = request.ExistingIssueId.IfPoss(i => Load<Issue>(request.ExistingIssueId));
            var matchingIssue = existingIssue == null 
                ? issues.FirstOrDefault(i => i.RulesMatch(error)) 
                : issues.FirstOrDefault(i => i.Id != existingIssue.Id && i.RulesMatch(error));

            Trace("Matching issue: {0}", existingIssue == null ? "NONE" : existingIssue.Id);

            //if we are re-ingesting an issues errors and we cant find another match, attach the error to the original issue
            if (matchingIssue == null && existingIssue != null)
            {
                Trace("Attach to existing home issue: {0}", existingIssue.Id);
                matchingIssue = existingIssue;
            }

            var issue = matchingIssue == null
                ? RegisterNewIssue(application, error)
                : UpdateMatchingIssue(matchingIssue.Id, application, error);
            
            Trace("Complete");

            return new ReceiveErrorResponse
            {
                IssueId = issue.Id
            };
        }

        private void StoreError(Error error)
        {
            Store(error);
        }

        private Application GetApplication(ReceiveErrorRequest request)
        {
            Application application;
            if (request.ApplicationId.IsNullOrEmpty())
            {
                application = _getApplicationByTokenQuery.Invoke(new GetApplicationByTokenRequest
                {
                    Token = request.Token,
                    CurrentUser = User.System()
                }).Application;
            }
            else
            {
                application = _getApplicationQuery.Invoke(new GetApplicationRequest
                {
                    Id = request.Error.ApplicationId,
                    OrganisationId = request.Error.OrganisationId,
                    CurrentUser = User.System()
                }).Application;
            }

            //dont process if we cant find the application or if the application is inactive
            if (application == null || !application.IsActive)
            {
                Trace("Failed to locate application {0}.", application == null ? "application is null" : "application is inactive");
                return null;
            }

            request.Error.ApplicationId = application.Id;
            request.Error.OrganisationId = application.OrganisationId;

            return application;
        }

        private Issue UpdateMatchingIssue(string issueId, Application application, Error error)
        {
            var issue = Load<Issue>(issueId);

            //if the matching issue is solved, send an email and set it back to Acknowledged
            if (issue.Status == IssueStatus.Solved || (issue.AlwaysNotify && issue.LastErrorUtc < DateTime.UtcNow.AddHours(-12)))
            {
                MaybeSendNotification(issue, application, NotificationType.NotifyOnNewInstanceOfSolvedClass, error);

                if(issue.Status == IssueStatus.Solved)
                    issue.Status = IssueStatus.Acknowledged;
            }

            //should the error be classified, yes if the issue has been acknowledged
            if (issue.Status != IssueStatus.Unacknowledged)
                error.Classified = true;

            error.IssueId = issue.Id;

            issue.ErrorCount++;
            issue.LastErrorUtc = DateTime.UtcNow;

            SetLimitStatus(application, issue);

            //always store the last error, but only if its a new error
            if (error.Id.IsNullOrEmpty())
                StoreError(error);

            //if LimitStatus == ErrorLimitStatus.Exceeded make a any errors over the limit UnloggedErrors
            if (issue.LimitStatus == ErrorLimitStatus.Exceeded)
            {
                _makeExceededErrorsUnloggedCommand.Invoke(new MakeExceededErrorsUnloggedRequest {IssueId = issue.Id});
            }

            return issue;
        }

        private void SetLimitStatus(Application application, Issue issue)
        {
            if (issue.ErrorCount >= _configuration.IssueErrorLimit)
            {
                if (issue.LimitStatus != ErrorLimitStatus.Exceeded)
                {
                    SendWarningNotification(application, new IssueErrorCountExceededEmailInfo
                    {
                        ErrorCount = _configuration.IssueErrorLimit,
                        IssueId = issue.FriendlyId,
                        IssueName = issue.Name
                    });
                }

                issue.LimitStatus = ErrorLimitStatus.Exceeded;
            }
        }

        private Issue RegisterNewIssue(Application application, Error error)
        {
            Trace("No issues found, adding new issue");

            //mark the error as unclassified
            error.Classified = false;

            var matchRuleFactory = _matchRuleFactoryFactory.Create(application.MatchRuleFactoryId);
            var rules = matchRuleFactory.Create(error).ToList();

            var issue = new Issue
            {
                Name = "{0} ({1})".FormatWith(error.ExceptionInfo.Type, DateTime.UtcNow.ToLocalTime().ToString("yyyy.MM.ddTHH.mm.ss")),
                Rules = rules,
                ApplicationId = application.Id,
                CreatedOnUtc = error.TimestampUtc,
                LastModifiedUtc = error.TimestampUtc,
                UserId = application.DefaultUserId,
                ErrorCount = 1,
                LastErrorUtc = error.TimestampUtc,
                OrganisationId = application.OrganisationId,
                History = new List<IssueHistory>
                {
                    new IssueHistory
                    {
                        DateAddedUtc = DateTime.UtcNow,
                        Message = Resources.CoreResources.HistoryIssueCreated.FormatWith(error.ExceptionInfo.Type, error.ExceptionInfo.MethodName, error.ExceptionInfo.Module, error.MachineName),
                        SystemMessage = true,
                    }
                },
                MatchPriority = MatchPriority.Low, //will get a low score when weighting the rules against errors,
                TestIssue = error.TestError,
            };

            Store(issue);
            Trace("Created issue: Id:={0}, Name:={1}", issue.Id, issue.Name);
            error.IssueId = issue.Id;
            MaybeSendNotification(issue, application, NotificationType.NotifyOnNewClassCreated, error);

            //tell the issue manager we have a new issue
            _receptionServiceIssueCache.Add(issue);

            StoreError(error);

            return issue;
        }

        private void MaybeSendNotification(Issue issue, Application application, NotificationType notificationType, Error instance)
        {
            var notification = application.GetNotification(notificationType);

            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                OrganisationId = application.OrganisationId,
                Groups = notification != null ? notification.Groups : new List<string>(),
                EmailInfo = issue.ToEmailInfo(notificationType, instance, application),
                Application = application
            });
        }

        private void SendWarningNotification(Application application, EmailInfoBase emailInfo)
        {
            var notification = application.GetNotification(NotificationType.NotifySystemWarnings);

            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                OrganisationId = application.OrganisationId,
                Groups = notification != null ? notification.Groups : new List<string>(),
                EmailInfo = emailInfo,
                Application = application
            });
        }     
    }

    public interface IReceiveErrorCommand : ICommand<ReceiveErrorRequest, ReceiveErrorResponse>
    { }

    public class ReceiveErrorRequest
    {
        public Error Error { get; set; }
        public string ApplicationId { get; set; }
        public string OrganisationId { get; set; }
        public string Token { get; set; }
        public string ExistingIssueId { get; set; }
        public bool ExecutingInProcess { get; set; }
    }

    public class ReceiveErrorResponse
    {
        public string IssueId { get; set; }
    }
}