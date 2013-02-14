﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Matching;
using Errordite.Core.WebApi;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.ActionSelectors;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using Errordite.Web.Models.Navigation;
using Newtonsoft.Json;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Resources;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class IssueController : ErrorditeController
    {
        private readonly IGetIssueQuery _getIssueQuery;
        private readonly IAdjustRulesCommand _adjustRulesCommand;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IUpdateIssueDetailsCommand _updateIssueDetailsCommand;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IDeleteIssueErrorsCommand _deleteIssueErrorsCommand;
        private readonly IDeleteIssueCommand _deleteIssueCommand;
        private readonly IGetIssueReportDataQuery _getIssueReportDataQuery;
        private readonly IAddCommentCommand _addCommentCommand;

        public IssueController(IGetIssueQuery getIssueQuery, 
            IAdjustRulesCommand adjustRulesCommand, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IUpdateIssueDetailsCommand updateIssueDetailsCommand, 
            ErrorditeConfiguration configuration, 
            IDeleteIssueErrorsCommand deleteIssueErrorsCommand, 
            IDeleteIssueCommand deleteIssueCommand,
            IGetIssueReportDataQuery getIssueReportDataQuery, 
            IAddCommentCommand addCommentCommand)
        {
            _getIssueQuery = getIssueQuery;
            _adjustRulesCommand = adjustRulesCommand;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _updateIssueDetailsCommand = updateIssueDetailsCommand;
            _configuration = configuration;
            _deleteIssueErrorsCommand = deleteIssueErrorsCommand;
            _deleteIssueCommand = deleteIssueCommand;
            _getIssueReportDataQuery = getIssueReportDataQuery;
            _addCommentCommand = addCommentCommand;
        }

        [
        ImportViewData, 
        GenerateBreadcrumbs(BreadcrumbId.Issue, BreadcrumbId.Issues, WebConstants.CookieSettings.IssueSearchCookieKey),
        PagingView(DefaultSort = Errordite.Core.CoreConstants.SortFields.TimestampUtc, DefaultSortDescending = true)
        ]
        public ActionResult Index(IssueErrorsPostModel postModel)
        {
            var viewModel = GetViewModel(postModel, GetSinglePagingRequest());

            if(viewModel == null)
            {
                Response.StatusCode = 404;
                return View("NotFound", new IssueNotFoundViewModel { Id = postModel.Id });
            }

            return View(viewModel);
        }

        private IssueViewModel GetViewModel(IssueErrorsPostModel postModel, PageRequestWithSort paging)
        {
            var issue = _getIssueQuery.Invoke(new GetIssueRequest { IssueId = postModel.Id, CurrentUser = Core.AppContext.CurrentUser }).Issue;

            if (issue == null)
                return null;

            var users = Core.GetUsers();
            var applications = Core.GetApplications();
            var assignedUser = users.Items.FirstOrDefault(u => u.Id == issue.UserId);

            int ii = 0;
            var ruleViewModels = issue.Rules.OfType<PropertyMatchRule>().Select(r => new RuleViewModel
            {
                ErrorProperty = r.ErrorProperty,
                StringOperator = r.StringOperator,
                Value = r.Value,
                Index = ii++,
                Properties = _configuration.GetRuleProperties(r.ErrorProperty)
            }).ToList();

            var rulesViewModel = new UpdateIssueViewModel
            {
                ApplicationId = issue.ApplicationId,
                Rules = ruleViewModels,
                Name = issue.Name,
                AdjustmentName = GetAdjustmentRejectsName(issue.Name),
                IssueId = issue.Id,
                Users = users.Items.ToSelectList(u => u.Id, u => "{0} {1}".FormatWith(u.FirstName, u.LastName), sortListBy: SortSelectListBy.Text, selected: u => u.Id == issue.UserId),
                Statuses = issue.Status.ToSelectedList(IssueResources.ResourceManager, false, issue.Status.ToString()),
                UserId = issue.UserId,
                Status = issue.Status,
                AlwaysNotify = issue.AlwaysNotify,
                Reference = issue.Reference,
            };

            var viewModel = new IssueViewModel
            {
                Details = new IssueDetailsViewModel
                {

                    ErrorCount = issue.ErrorCount,
                    LastErrorUtc = issue.LastErrorUtc,
                    FirstErrorUtc = issue.CreatedOnUtc,
                    UserName = assignedUser == null ? string.Empty : assignedUser.FullName,
                    ApplicationName = applications.Items.First(a => a.Id == issue.ApplicationId).Name,
                    ErrorLimitStatus = IssueResources.ResourceManager.GetString("ErrorLimitStatus_{0}".FormatWith(issue.LimitStatus)),
                    TestIssue = issue.TestIssue,
                    IssueId = issue.Id
                },
                Errors = GetErrorsViewModel(postModel, paging),
                Update = rulesViewModel,
                Tab = postModel.Tab
            };

            //dont let users set an issue to unacknowledged
            if (issue.Status != IssueStatus.Unacknowledged)
            {
                var statuses = viewModel.Update.Statuses.ToList();
                statuses.Remove(viewModel.Update.Statuses.First(s => s.Value == IssueStatus.Unacknowledged.ToString()));
                viewModel.Update.Statuses = statuses;
            }

            return viewModel;
        }

        [ImportViewData]
        public ActionResult GetReportData(IssueDetailsPostModel postModel)
        {
            var request = new GetIssueReportDataRequest
            {
                CurrentUser = Core.AppContext.CurrentUser,
                IssueId = postModel.IssueId,
                StartDate = DateTime.UtcNow.AddDays(-30).Date,
                EndDate = DateTime.UtcNow.Date
            };

            if (postModel.DateRange.IsNotNullOrEmpty())
            {
                string[] dates = postModel.DateRange.Split('|');

                DateTime startDate;
                DateTime endDate;

                if (DateTime.TryParse(dates[0], out startDate) && DateTime.TryParse(dates[1], out endDate))
                {
                    request.StartDate = startDate;
                    request.EndDate = endDate;
                }
            }

            var data = _getIssueReportDataQuery.Invoke(request).Data;

            return new PlainJsonNetResult(data, true);
        }

        [ImportViewData]
        public ActionResult NotFound(IssueNotFoundViewModel viewModel)
        {
            return View(viewModel);
        }

        [PagingView(DefaultSort = Errordite.Core.CoreConstants.SortFields.TimestampUtc, DefaultSortDescending = true), ImportViewData]
        public ActionResult Errors(ErrorCriteriaPostModel postModel)
        {
            var paging = GetSinglePagingRequest();
            var model = GetErrorsViewModel(postModel, paging);
            return PartialView("Errors/ErrorItems", model);
        }

        private ErrorCriteriaViewModel GetErrorsViewModel(ErrorCriteriaPostModel postModel, PageRequestWithSort paging)
        {
            var request = new GetApplicationErrorsRequest
            {
                OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                IssueId = postModel.IssueId,
                Paging = paging,
                UserTimezoneId = AppContext.CurrentUser.EffectiveTimezoneId(),
            };

            if (postModel.DateRange.IsNotNullOrEmpty())
            {
                string[] dates = postModel.DateRange.Split('|');

                DateTime startDate;
                DateTime endDate;

                if (DateTime.TryParse(dates[0], out startDate) && DateTime.TryParse(dates[1], out endDate))
                {
                    request.StartDate = startDate;
                    request.EndDate = endDate.AddDays(1).AddMinutes(-1);
                }
            }

            var errors = _getApplicationErrorsQuery.Invoke(request).Errors;

            var model = new ErrorCriteriaViewModel
            {
                Action = "errors",
                Controller = "issue",
                DateRange = postModel.DateRange,
                Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, errors.PagingStatus, paging),
                Errors = errors.Items.Select(e => new ErrorInstanceViewModel { Error = e, HideIssues = true }).ToList(),
                ApplicationId = postModel.ApplicationId,
                HideIssues = true,
                IssueId = postModel.IssueId,
                Applications = Core.GetApplications().Items.ToSelectList(a => a.FriendlyId, a => a.Name, u => u.FriendlyId == postModel.ApplicationId, Resources.Shared.Application, string.Empty, SortSelectListBy.Text),
                Sort = paging.Sort,
                SortDescending = paging.SortDescending,
            };

            model.Paging.Tab = IssueTab.Details.ToString();
            return model;
        }

        public ActionResult History(string issueId)
        {
            var issueMemoizer = new LocalMemoizer<string, Issue>(id =>
                    _getIssueQuery.Invoke(new GetIssueRequest { CurrentUser = Core.AppContext.CurrentUser, IssueId = id }).Issue);

            var history = Core.Session.Raven.Query<HistoryDocument, History_Search>()
                .Where(h => h.IssueId == Issue.GetId(issueId))
                .As<IssueHistory>()
                .ToList();

            var users = Core.GetUsers();

            var results = history.OrderByDescending(h => h.DateAddedUtc).Select(h =>
                {
                    var user = users.Items.FirstOrDefault(u => u.Id == h.UserId);
                    return RenderPartial("Issue/HistoryItem", new IssueHistoryItemViewModel
                    {
                        Message = h.GetMessage(users.Items, issueMemoizer, GetIssueLink),
                        DateAddedUtc = h.DateAddedUtc,
                        UserEmail = user != null ? user.Email : string.Empty,
                        Username = user != null ? user.FullName : string.Empty,
                        SystemMessage = h.SystemMessage,
                    });
                });

            return new JsonSuccessResult(results, allowGet:true);
        }

        private string GetIssueLink(string issueId)
        {
            return "<a href='{0}'>Issue {1}</a>".FormatWith(Url.Issue(issueId ?? "0"), IdHelper.GetFriendlyId(issueId ?? "0"));
        }

        private const string RejectPrefix = "Adjustment Rejects";
        private static readonly Regex _adjustmentRejectNameRegex = new Regex(@"^(?'ar'{0}( \((?'count'\d+)\))?:)?(?'main'.*)".FormatWith(RejectPrefix), RegexOptions.IgnoreCase);

        private string GetAdjustmentRejectsName(string issueName)
        {
            var name = _adjustmentRejectNameRegex.Replace(issueName, m =>
            {
                var arGroup = m.Groups["ar"];
                string prefix;
                if (arGroup.Success)
                {
                    var countGroup = m.Groups["count"];
                    int count = countGroup.Success
                        ? int.Parse(countGroup.Value) + 1
                        : 2;

                    prefix = string.Format("{0} ({1}): ", RejectPrefix, count);
                }
                else
                {
                    prefix = "{0}: ".FormatWith(RejectPrefix);
                }
                return prefix + m.Groups["main"].Value;
            });
            return name;
        }

        [HttpPost, ValidateInput(false), ActionName("AdjustRules"), IfButtonClicked("WhatIf")]
        public ActionResult WhatIfAdjustRules(UpdateIssuePostModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.IssueId, tab = IssueTab.Rules.ToString() });
            }

            var result = _adjustRulesCommand.Invoke(new AdjustRulesRequest
                {
                    IssueId = postModel.IssueId,
                    ApplicationId = postModel.ApplicationId,
                    Rules =
                        postModel.Rules.Select(
                            r => (IMatchRule) new PropertyMatchRule(r.ErrorProperty, r.StringOperator, r.Value))
                                 .ToList(),
                    CurrentUser = Core.AppContext.CurrentUser,
                    NewIssueName = postModel.AdjustmentName,
                    OriginalIssueName = postModel.Name,
                    WhatIf = true,
                });

            return new JsonSuccessResult(
                new {
                    total = result.ErrorsMatched + result.ErrorsNotMatched,
                    matched = result.ErrorsMatched,
                    notmatched = result.ErrorsNotMatched,
                });
        }

        [HttpPost, ExportViewData, ValidateInput(false), IfButtonClicked("AdjustRules")]
        public ActionResult Update(UpdateIssuePostModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.IssueId, tab = IssueTab.Rules.ToString() }); 
            }

            try
            {
                var updateResult = _updateIssueDetailsCommand.Invoke(new UpdateIssueDetailsRequest
                {
                    IssueId = postModel.IssueId,
                    Status = postModel.Status,
                    Name = postModel.Name,
                    CurrentUser = Core.AppContext.CurrentUser,
                    AlwaysNotify = postModel.AlwaysNotify,
                    Reference = postModel.Reference,
                    AssignedUserId = postModel.UserId
                });

                if (updateResult.Status == UpdateIssueDetailsStatus.IssueNotFound)
                {
                    return RedirectToAction("notfound", new { FriendlyId = postModel.IssueId.GetFriendlyId() });
                }

                var result = _adjustRulesCommand.Invoke(new AdjustRulesRequest
                {
                    IssueId = postModel.IssueId,
                    ApplicationId = postModel.ApplicationId,
                    Rules = postModel.Rules.Select(r => (IMatchRule)new PropertyMatchRule(r.ErrorProperty, r.StringOperator, r.Value)).ToList(),
                    CurrentUser = Core.AppContext.CurrentUser,
                    NewIssueName = postModel.AdjustmentName,
                    OriginalIssueName = postModel.Name
                }); 
                
                switch (result.Status)
                {
                    case AdjustRulesStatus.IssueNotFound:
                        return RedirectToAction("notfound", new { FriendlyId = postModel.IssueId.GetFriendlyId() });
                    case AdjustRulesStatus.Ok:
                        ConfirmationNotification(new MvcHtmlString("Rules adjusted successfully. Of {0} error{1}, {2} still match{3} and remain{4} attached to the issue.{5}".FormatWith(
                            result.ErrorsMatched + result.ErrorsNotMatched,
                            result.ErrorsNotMatched + result.ErrorsNotMatched == 1 ? "" : "s",
                            result.ErrorsNotMatched == 0 ? "all" : result.ErrorsMatched.ToString(),
                            result.ErrorsMatched == 1 ? "es" : "",
                            result.ErrorsMatched == 1 && result.ErrorsNotMatched > 0 ? "s" : "",
                            result.ErrorsNotMatched > 0
                                ? " The {0} that did not match {1} been attached to newly created issue # <a href='{2}'>{3}</a>".FormatWith(
                                    result.ErrorsNotMatched,
                                    result.ErrorsNotMatched == 1 ? "has" : "have",
                                    Url.Issue(result.UnmatchedIssueId),
                                    result.UnmatchedIssueId)
                                : string.Empty
                            )));
                        break;
                    default:
                        return RedirectWithViewModel(postModel, "index", result.Status.MapToResource(Rules.ResourceManager), false, new { id = postModel.IssueId, tab = IssueTab.Rules.ToString() });
                }

                return RedirectToAction("index", new { id = result.IssueId, tab = IssueTab.Rules.ToString() });
            }
            catch (ConcurrencyException e)
            {
                ErrorNotification("A background process modified this issues data at the same time as you requested to adjust the rules, please try again.");
                return RedirectToAction("index", new { id = postModel.IssueId, tab = IssueTab.Rules.ToString() });
            }
        }

		[HttpPost, ExportViewData]
		public ActionResult AddComment(AddCommentViewModel postModel)
		{
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.IssueId, tab = IssueTab.History.ToString() });
            }

            var result = _addCommentCommand.Invoke(new AddCommentRequest
			{
				IssueId = postModel.IssueId,
				CurrentUser = Core.AppContext.CurrentUser,
				Comment = postModel.Comment
			});

			if (result.Status == AddCommentStatus.IssueNotFound)
			{
				return RedirectToAction("notfound", new { FriendlyId = postModel.IssueId.GetFriendlyId() });
			}

			ConfirmationNotification("Comment was added to the history successfully");
			return RedirectToAction("index", new { id = postModel.IssueId, tab = IssueTab.History.ToString() });
		}

        [HttpPost, ExportViewData]
        public ActionResult Purge(string issueId)
        {
            try
            {
                _deleteIssueErrorsCommand.Invoke(new DeleteIssueErrorsRequest
                {
                    IssueId = issueId,
                    CurrentUser = Core.AppContext.CurrentUser
                });

                ConfirmationNotification(IssueResources.IssuePurged);
            }
            catch (ConcurrencyException e)
            {
                ErrorNotification("A background process modified this issues data at the same time as you requested to delete the errors, please try again.");
            }

            if (Request.IsAjaxRequest())
                return new JsonSuccessResult();

            return RedirectToAction("index", new { id = issueId, tab = IssueTab.History.ToString() });
        }

        [HttpPost, ExportViewData]
        public ActionResult Import(string issueId)
        {
            var request = new ReprocessIssueErrorsRequest
            {
                IssueId = issueId,
                CurrentUser = Core.AppContext.CurrentUser
            };

            var httpTask = Core.Session.ReceptionServiceHttpClient.PostJsonAsync("ReprocessIssueErrors", request);
            httpTask.Wait();

            if (!httpTask.Result.IsSuccessStatusCode)
            {
                ErrorNotification("An error has occured while attempting to reprocess errors, its likely this is a concurrency problem so please try again.");
            }
            else
            {
                var contentTask = httpTask.Result.Content.ReadAsStringAsync();
                contentTask.Wait();

                var response = JsonConvert.DeserializeObject<ReprocessIssueErrorsResponse>(contentTask.Result);

                if (response.Status == ReprocessIssueErrorsStatus.NotAuthorised)
                {
                    throw new ErrorditeAuthorisationException(new Core.Domain.Error.Issue { Id = issueId }, Core.AppContext.CurrentUser);
                }

                ConfirmationNotification(response.GetMessage(Errordite.Core.Domain.Error.Issue.GetId(issueId)));
            }

            return RedirectToAction("index", new { id = issueId, tab = IssueTab.Details.ToString() });
        }

        [HttpPost, ExportViewData]
        public ActionResult Delete(string issueId)
        {
            _deleteIssueCommand.Invoke(new DeleteIssueRequest
            {
                IssueId = issueId,
                CurrentUser = Core.AppContext.CurrentUser
            });

            ConfirmationNotification(IssueResources.DeleteIssueStatus_Ok.FormatWith(issueId));

            return RedirectToAction("index", "issues");
        }
    }
}
