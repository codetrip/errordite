using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
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
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

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
            var issues = _receptionServiceIssueCache.GetIssues(application.Id, application.OrganisationId);
            var existingIssue = request.ExistingIssueId.IfPoss(i => Load<Issue>(request.ExistingIssueId));
            var matchingIssue = existingIssue == null 
                ? issues.FirstOrDefault(i => i.RulesMatch(error)) 
                : issues.FirstOrDefault(i => i.Id != existingIssue.Id && i.RulesMatch(error));

            Trace("Matching issue: {0}", existingIssue == null ? "NONE" : existingIssue.Id);

            //if we are re-ingesting an issues errors and we cant find another match, attach the error to the original issue
            if (matchingIssue == null && existingIssue != null)
            {
                Trace("No issues matched, attaching to the existing issue with Id:={0}", existingIssue.Id);
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

            issue.ErrorCount++;
            issue.LastErrorUtc = DateTime.UtcNow;

            SetLimitStatus(application, issue);

            //should the error be classified, yes if the issue has been acknowledged
            if (issue.Status != IssueStatus.Unacknowledged)
                error.Classified = true;

            Trace("Assigning issue Id to error with Id:={0}, Existing Error IssueId:={1}, New IssueId:={2}", error.Id, error.IssueId, issue.Id);
            error.IssueId = issue.Id;

            //only store the error is it is a new error, not the result of reprocessing
            if (error.Id.IsNullOrEmpty())
            {
                Trace("Its a new error, so Store it");
                Store(error);
            }

            if (issue.RulesMatch(error))
            {
                Trace("Error definately matches issue we are about to assign it to");
            }
            else
            {
                throw new InvalidOperationException(
                    "ERROR DOES NOT MATCH RULES Error with Id:={0} does not match issue {1} which it has been assigned to, applicationID:={2}"
                        .FormatWith(error.Id, issue.Id, issue.ApplicationId));
            }

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
                        //Message = Resources.CoreResources.HistoryIssueCreated.FormatWith(error.ExceptionInfo.Type, error.ExceptionInfo.MethodName, error.ExceptionInfo.Module, error.MachineName),
                        Type = HistoryItemType.AutoCreated,
                        ExceptionType = error.ExceptionInfo.Type,
                        ExceptionMethod = error.ExceptionInfo.MethodName,
                        ExceptionModule = error.ExceptionInfo.Module,
                        ExceptionMachine = error.MachineName,
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

            //tell the issue cache we have a new issue
            _receptionServiceIssueCache.Add(issue);

            Store(error);

            if (issue.RulesMatch(error))
            {
                Trace("Error definately matches issue we are about to assign it to");
            }
            else
            {
                throw new InvalidOperationException(
                    "ERROR DOES NOT MATCH RULES Error with Id:={0} does not match issue {1} which it has been assigned to, applicationID:={2}"
                        .FormatWith(error.Id, issue.Id, issue.ApplicationId));
            }

            return issue;
        }

        private void MaybeSendNotification(Issue issue, Application application, NotificationType notificationType, Error instance)
        {
            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                OrganisationId = application.OrganisationId,
                Groups = application.NotificationGroups ?? new List<string>(),
                EmailInfo = issue.ToEmailInfo(notificationType, instance, application),
                Application = application
            });
        }

        private void SendWarningNotification(Application application, EmailInfoBase emailInfo)
        {
            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                OrganisationId = application.OrganisationId,
                Groups = application.NotificationGroups ?? new List<string>(),
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
    }

    public class ReceiveErrorResponse
    {
        public string IssueId { get; set; }
    }
}