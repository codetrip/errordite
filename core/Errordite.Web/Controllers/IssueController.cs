using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Matching;
using Errordite.Core.Resources;
using Errordite.Core.Users.Queries;
using Errordite.Core.WebApi;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using Errordite.Web.Models.Navigation;
using Newtonsoft.Json;
using Raven.Abstractions.Exceptions;
using Resources;
using CoreConstants = Errordite.Core.CoreConstants;

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
        private readonly IPurgeIssueCommand _purgeIssueCommand;
        private readonly IDeleteIssueCommand _deleteIssueCommand;
        private readonly IGetUserQuery _getUserQuery;
        private readonly IGetIssueReportDataQuery _getIssueReportDataQuery;
        private readonly IAddCommentCommand _addCommentCommand;

        public IssueController(IGetIssueQuery getIssueQuery, 
            IAdjustRulesCommand adjustRulesCommand, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IUpdateIssueDetailsCommand updateIssueDetailsCommand, 
            ErrorditeConfiguration configuration, 
            IPurgeIssueCommand purgeIssueCommand, 
            IDeleteIssueCommand deleteIssueCommand,
			IGetUserQuery getUserQuery, 
            IGetIssueReportDataQuery getIssueReportDataQuery, 
            IAddCommentCommand addCommentCommand)
        {
            _getIssueQuery = getIssueQuery;
            _adjustRulesCommand = adjustRulesCommand;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _updateIssueDetailsCommand = updateIssueDetailsCommand;
            _configuration = configuration;
            _purgeIssueCommand = purgeIssueCommand;
            _deleteIssueCommand = deleteIssueCommand;
            _getUserQuery = getUserQuery;
            _getIssueReportDataQuery = getIssueReportDataQuery;
            _addCommentCommand = addCommentCommand;
        }

        [ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Issue, BreadcrumbId.Issues, WebConstants.CookieSettings.IssueSearchCookieKey)]
        public ActionResult Index(string id, IssueTab tab = IssueTab.Details)
        {
            var viewModel = GetViewModel(new ErrorCriteriaPostModel
            {
                IssueId = id
            }, tab);

            if(viewModel == null)
            {
                Response.StatusCode = 404;
                return View("NotFound", new IssueNotFoundViewModel { Id = id });
            }

            //re-apply the posted model
            if (ViewData.Model != null)
            {
                if(ViewData.Model is IssueDetailsPostModel)
                {
                    var postedModel = (IssueDetailsPostModel)ViewData.Model;
                    viewModel.Details.Status = postedModel.Status;
                    viewModel.Details.UserId = postedModel.UserId;
                    viewModel.Details.Name = postedModel.Name;
                    viewModel.Details.Reference = postedModel.Reference;
                    viewModel.Details.Comment = postedModel.Comment;
                    viewModel.Details.AlwaysNotify = postedModel.AlwaysNotify;
                    viewModel.Details.DateRange = "{0} - {1}".FormatWith(DateTime.UtcNow.AddDays(-30).ToString("MMMM d, yyyy"), DateTime.UtcNow.ToString("MMMM d, yyyy"));
                }
                else if (ViewData.Model is IssueRulesPostModel)
                {
                    var postedModel = (IssueRulesPostModel)ViewData.Model;

                    foreach (var rulesViewModel in postedModel.Rules)
                    {
                        rulesViewModel.Properties = _configuration.GetRuleProperties(rulesViewModel.ErrorProperty);
                    }
                }
            }

            return View(viewModel);
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

        [PagingView(DefaultSort = CoreConstants.SortFields.TimestampUtc, DefaultSortDescending = true), ImportViewData]
        public ActionResult Errors(ErrorCriteriaPostModel postModel)
        {
            var paging = GetSinglePagingRequest();

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

            model.Paging.Tab = IssueTab.Errors.ToString();

            return PartialView("Errors/ErrorItems", model);
        }

        private IssueViewModel GetViewModel(ErrorCriteriaPostModel postModel, IssueTab tab)
        {
            var issue = _getIssueQuery.Invoke(new GetIssueRequest { IssueId = postModel.IssueId, CurrentUser = Core.AppContext.CurrentUser }).Issue;
            
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

            //var priorities = issue.MatchPriority.ToSelectedList(Issue.ResourceManager, false, issue.MatchPriority.ToString());

            var rulesViewModel = new IssueRulesPostModel
            {
                Id = issue.Id,
                ApplicationId = issue.ApplicationId,
                Rules = ruleViewModels,
                IssueNameAfterUpdate = issue.Name,
                UnmatchedIssueName = GetAdjustmentRejectsName(issue.Name),
            };

            var userMemoizer =
                new LocalMemoizer<string, User>(
                    id =>
                    _getUserQuery.Invoke(new GetUserRequest {OrganisationId = issue.OrganisationId, UserId = id}).User);
            var issueMemoizer = new LocalMemoizer<string, Issue>(id =>
                    _getIssueQuery.Invoke(new GetIssueRequest { CurrentUser = Core.AppContext.CurrentUser, IssueId = id }).Issue);

            var viewModel = new IssueViewModel
            {
                Details = new IssueDetailsViewModel
                {
                    IssueId = issue.FriendlyId,
                    Name = issue.Name,
                    ErrorCount = issue.ErrorCount,
                    LastErrorUtc = issue.LastErrorUtc,
                    FirstErrorUtc = issue.CreatedOnUtc,
                    Status = issue.Status,
                    UserName = assignedUser == null ? string.Empty : assignedUser.FullName,
                    Users = users.Items.ToSelectList(u => u.Id, u => "{0} {1}".FormatWith(u.FirstName, u.LastName), sortListBy: SortSelectListBy.Text, selected: u => u.Id == issue.UserId),
                    Statuses = issue.Status.ToSelectedList(IssueResources.ResourceManager, false, issue.Status.ToString()),
                    SampleError = _getApplicationErrorsQuery.Invoke(new GetApplicationErrorsRequest
	                {
		                IssueId = issue.Id,
						Paging = new PageRequestWithSort(1, 1),
						OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
						UserTimezoneId = AppContext.CurrentUser.EffectiveTimezoneId()
	                }).Errors.Items.FirstOrDefault(),
                    UserId = issue.UserId,
                    ApplicationName = applications.Items.First(a => a.Id == issue.ApplicationId).Name,
                    ErrorLimitStatus = IssueResources.ResourceManager.GetString("ErrorLimitStatus_{0}".FormatWith(issue.LimitStatus)),
                    AlwaysNotify = issue.AlwaysNotify,
                    Reference = issue.Reference,
                    History = issue.History.OrderByDescending(h => h.DateAddedUtc).Select(h => 
                    {
                        var user = users.Items.FirstOrDefault(u => u.Id == h.UserId);
                        return new IssueHistoryItemViewModel
                        {
                            Message = GetMessage(h, userMemoizer, issueMemoizer, issue),
                            DateAddedUtc = h.DateAddedUtc,
                            UserEmail = user != null ? user.Email : string.Empty,
                            Username = user != null ? user.FullName : string.Empty,
                            SystemMessage = h.SystemMessage,
                            Reference = h.Reference
                        };
                    }).ToList(),
                    TestIssue = issue.TestIssue,
                    DateRange = "{0} - {1}".FormatWith(DateTime.UtcNow.AddDays(-30).ToString("MMMM d, yyyy"), DateTime.UtcNow.ToString("MMMM d, yyyy"))
                },
                Errors = new ErrorCriteriaViewModel
                {
                    IssueId = issue.Id,
                    ApplicationId = issue.ApplicationId,
                    HideIssues = true,
                    Controller = "issue"
                },
                Rules = rulesViewModel,
                Tab = tab
            };

            //dont let users set an issue to unacknowledged
            if(issue.Status != IssueStatus.Unacknowledged)
            {
                var statuses = viewModel.Details.Statuses.ToList();
                statuses.Remove(viewModel.Details.Statuses.First(s => s.Value == IssueStatus.Unacknowledged.ToString()));
                viewModel.Details.Statuses = statuses;
            }

            return viewModel;
        }

        private class LocalMemoizer<TKey, TValue>
        {
            private Dictionary<TKey, TValue> _store = new Dictionary<TKey, TValue>();
            private Func<TKey, TValue> _func;

            public LocalMemoizer(Func<TKey, TValue> func)
            {
                _func = func;
            }

            public TValue Get(TKey key)
            {
                TValue ret;
                if (!_store.TryGetValue(key, out ret))
                {
                    ret = _func(key);
                    _store[key] = ret;
                }

                return ret;
            }
        }

        private string GetMessage(IssueHistory h, LocalMemoizer<string, User> userMemoizer, LocalMemoizer<string, Issue> issueMemoizer, Issue issue)
        {
            var user = h.UserId.IfPoss(id => userMemoizer.Get(h.UserId));

            switch (h.Type)
            {
                case HistoryItemType.CreatedByRuleAdjustment:
                    return CoreResources.HistoryCreatedByRulesAdjustment.FormatWith(IdHelper.GetFriendlyId(h.SpawningIssueId),
                        user.IfPoss(u => u.FullName),
                        user.IfPoss(u => u.FirstName));
                case HistoryItemType.ManuallyCreated:
                    return CoreResources.HistoryIssueCreatedBy.FormatWith(user.IfPoss(u => u.FullName), user.IfPoss(u => u.FirstName));
                case HistoryItemType.BatchStatusUpdate:
                    return "{0}{1}{2}".FormatWith(
                        CoreResources.HistoryIssueStatusUpdated.FormatWith(h.PreviousStatus, h.NewStatus, user.IfPoss(u => u.FullName), user.IfPoss(u => u.Email)),
                        h.AssignedToUserId == null ? "" : "{0}Assigned to {1} ({2})".FormatWith(
                                Environment.NewLine,
                                userMemoizer.Get(h.AssignedToUserId).IfPoss(u => u.FullName),
                                userMemoizer.Get(h.AssignedToUserId).IfPoss(u => u.Email)),
                        h.Comment == null ? "" : Environment.NewLine + h.Comment);
                case HistoryItemType.MergedTo:
                    return CoreResources.HistoryIssueMerged.FormatWith(IdHelper.GetFriendlyId(h.SpawningIssueId), issueMemoizer.Get(h.SpawningIssueId).IfPoss(i => i.Name, "DELETED"));
                case HistoryItemType.ErrorsPurged:
                    return CoreResources.HistoryIssuePurged.FormatWith(user.IfPoss(u => u.FullName), user.IfPoss(u => u.Email));
                case HistoryItemType.ErrorsReprocessed:
                    return CoreResources.HistoryIssueErrorsReceivedAgain.FormatWith(
                        user.IfPoss(u => u.FullName),
                        user.IfPoss(u => u.Email),
                        new ReprocessIssueErrorsResponse() { AttachedIssueIds = h.ReprocessingResult, Status = ReprocessIssueErrorsStatus.Ok }.GetMessage
                            (issue.Id));
                case HistoryItemType.Comment:
                    return h.Comment; //TODO - record more - like, what got updated
                case HistoryItemType.RulesAdjustedCreatedNewIssue:
                    return CoreResources.HistoryRulesAdjusted.FormatWith(user.IfPoss(u => u.FullName), user.IfPoss(u => u.Email), h.SpawnedIssueId);
				case HistoryItemType.AutoCreated:
		            return "Issue created by new error";
                default:
                    return h.Type + " " + h.Comment;
            }
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

        [HttpPost, ExportViewData, ValidateInput(false)]
        public ActionResult AdjustRules(IssueRulesPostModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.Id, tab = IssueTab.Rules.ToString() }); 
            }

            try
            {
                var result = _adjustRulesCommand.Invoke(new AdjustRulesRequest
                {
                    IssueId = postModel.Id,
                    ApplicationId = postModel.ApplicationId,
                    Rules = postModel.Rules.Select(r => (IMatchRule)new PropertyMatchRule(r.ErrorProperty, r.StringOperator, r.Value)).ToList(),
                    CurrentUser = Core.AppContext.CurrentUser,
                    NewIssueName = postModel.UnmatchedIssueName,
                    OriginalIssueName = postModel.IssueNameAfterUpdate
                }); 
                
                switch (result.Status)
                {
                    case AdjustRulesStatus.IssueNotFound:
                        return RedirectToAction("notfound", new { FriendlyId = postModel.Id.GetFriendlyId() });
                    case AdjustRulesStatus.Ok:
                        ConfirmationNotification(new MvcHtmlString("Rules adjusted successfully. Of {0} errors, {1} still match{2} and remain{3} attached to the issue.{4}".FormatWith(
                            result.ErrorsMatched + result.ErrorsNotMatched,
                            result.ErrorsNotMatched == 0 ? "all" : result.ErrorsMatched.ToString(),
                            result.ErrorsMatched == 1 ? "es" : "",
                            result.ErrorsMatched == 1 ? "s" : "",
                            result.ErrorsNotMatched > 0
                                ? " The {0} that did not match {1} been attached to newly created issue # <a href='{2}'>{3}</a>".FormatWith(
                                    result.ErrorsNotMatched,
                                    result.ErrorsNotMatched == 1 ? "has" : "have",
                                    Url.Issue(result.UnmatchedIssueId),
                                    result.UnmatchedIssueId)
                                : string.Empty
                            )));
                        break;
                    case AdjustRulesStatus.RulesMatchedOtherIssue:
                        ConfirmationNotification(Rules.RulesMatchedOtherIssue);
                        return RedirectToAction("merge", "issues", new { leftIssueId = result.IssueId, rightIssueId = result.MatchingIssueId });
                    case AdjustRulesStatus.AutoMergedWithOtherIssue:
                        ConfirmationNotification(Rules.AutoMergedWithOtherIssue);
                        break;
                    default:
                        return RedirectWithViewModel(postModel, "index", result.Status.MapToResource(Rules.ResourceManager), false, new { id = postModel.Id, tab = IssueTab.Rules.ToString() });
                }

                return RedirectToAction("index", new { id = result.IssueId, tab = IssueTab.Rules.ToString() });
            }
            catch (ConcurrencyException e)
            {
                ErrorNotification("A background process modified this issues data at the same time as you requested to adjust the rules, please try again.");
                return RedirectToAction("index", new { id = postModel.Id, tab = IssueTab.Rules.ToString() });
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
        public ActionResult AdjustDetails(IssueDetailsPostModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.IssueId, tab = IssueTab.Details.ToString() });
            }

            var result = _updateIssueDetailsCommand.Invoke(new UpdateIssueDetailsRequest
            {
                IssueId = postModel.IssueId,
                Status = postModel.Status,
                Name = postModel.Name,
                CurrentUser = Core.AppContext.CurrentUser,
                AlwaysNotify = postModel.AlwaysNotify,
                Reference = postModel.Reference,
                AssignedUserId = postModel.UserId
            });

            if (result.Status == UpdateIssueDetailsStatus.IssueNotFound)
            {
                return RedirectToAction("notfound", new { FriendlyId = postModel.IssueId.GetFriendlyId() });
            }

            ConfirmationNotification(IssueResources.DetailsUpdated);
            return RedirectToAction("index", new { id = postModel.IssueId, tab = IssueTab.Details.ToString() });
        }

        [HttpPost, ExportViewData]
        public ActionResult Purge(string issueId)
        {
            try
            {
                _purgeIssueCommand.Invoke(new PurgeIssueRequest
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
                ErrorNotification("An error has occured while attempting to reprocess errors, please try again.");
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
