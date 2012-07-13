using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core.Configuration;
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
using Errordite.Web.Extensions;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;
using Errordite.Web.Models.Navigation;
using Newtonsoft.Json;
using Resources;
using CoreConstants = Errordite.Core.CoreConstants;
using Issue = Resources.Issue;
using Errordite.Core.Extensions;

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
        private readonly IReprocessIssueErrorsCommand _reprocessIssueErrorsCommand;
        private readonly IDeleteIssueCommand _deleteIssueCommand;

        public IssueController(IGetIssueQuery getIssueQuery, 
            IAdjustRulesCommand adjustRulesCommand, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IUpdateIssueDetailsCommand updateIssueDetailsCommand, 
            ErrorditeConfiguration configuration, 
            IPurgeIssueCommand purgeIssueCommand, 
            IReprocessIssueErrorsCommand reprocessIssueErrorsCommand, 
            IDeleteIssueCommand deleteIssueCommand)
        {
            _getIssueQuery = getIssueQuery;
            _adjustRulesCommand = adjustRulesCommand;
            _getApplicationErrorsQuery = getApplicationErrorsQuery;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _updateIssueDetailsCommand = updateIssueDetailsCommand;
            _configuration = configuration;
            _purgeIssueCommand = purgeIssueCommand;
            _reprocessIssueErrorsCommand = reprocessIssueErrorsCommand;
            _deleteIssueCommand = deleteIssueCommand;
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
                    viewModel.Details.Priority = postedModel.Priority;
                    viewModel.Details.Reference = postedModel.Reference;
                    viewModel.Details.Comment = postedModel.Comment;
                    viewModel.Details.AlwaysNotify = postedModel.AlwaysNotify;
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
        public ActionResult GetReportData(string issueId)
        {
            var results = Core.Session.Raven.Query<ByHourReduceResult, Errors_ByIssueByHour>()
                .Where(i => i.IssueId == Errordite.Core.Domain.Error.Issue.GetId(issueId))
                .AsEnumerable()
                .Select(h => new
                                 {
                                     hour = (DateTime.Today + new TimeSpan(h.Hour.Hour, 0, 0)).ToLocal().Hour,
                                     count = h.Count,
                                 });

            var allHourResults =
                (from hour in Enumerable.Range(0, 24)
                 join result in results on hour equals result.hour into countsTemp
                 from hours in countsTemp.DefaultIfEmpty()
                 select new {hour, count = hours.IfPoss(h => h.count)}).ToArray();

            return new JsonSuccessResult(JsonConvert.SerializeObject(new
            {
                ticks = allHourResults.Select(h => h.hour.ToString("0")),
                series = new[] { allHourResults.Select(h => h.count).ToArray() },
            }), true);
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
                ApplicationId = postModel.ApplicationId,
                OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                IssueId = postModel.IssueId,
                Paging = paging,
                StartDate = postModel.StartDate,
                EndDate = postModel.EndDate,
                Query = postModel.Query, 
                UserTimezoneId = AppContext.CurrentUser.EffectiveTimezoneId(),
            };

            var errors = _getApplicationErrorsQuery.Invoke(request).Errors;

            var model = new ErrorCriteriaViewModel
            {
                Action = "errors",
                Controller = "issue",
                StartDate = postModel.StartDate,
                EndDate = postModel.EndDate,
                Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, errors.PagingStatus, paging),
                Errors = errors.Items.Select(e => new ErrorInstanceViewModel { Error = e, HideIssues = true }).ToList(),
                ApplicationId = postModel.ApplicationId,
                HideIssues = true,
                IssueId = postModel.IssueId,
                Applications = Core.GetApplications().Items.ToSelectList(a => a.FriendlyId, a => a.Name, u => u.FriendlyId == postModel.ApplicationId, Resources.Shared.Application, string.Empty, SortSelectListBy.Text)
               
            };

            model.Paging.Tab = IssueTab.Errors.ToString();

            return PartialView("Errors/ErrorCriteria", model);
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

            var rulesViewModel = new IssueRulesViewModel
            {
                Id = issue.Id,
                ApplicationId = issue.ApplicationId,
                Rules = ruleViewModels,
                IssueNameAfterUpdate = issue.Name,
                MatchPriorityAfterUpdate = issue.Status == IssueStatus.Unacknowledged ? MatchPriority.Medium : issue.MatchPriority, //assume that adjusting an issue means we want it to catch errors
                UnmatchedIssueName = GetAdjustmentRejectsName(issue.Name),
                //Priorities = priorities,
                UnmatchedIssuePriority = issue.MatchPriority,
            };

            var viewModel = new IssueViewModel
            {
                Details = new IssueDetailsViewModel
                {
                    IssueId = issue.FriendlyId,
                    Name = issue.Name,
                    ErrorCount = issue.ErrorCount,
                    LastErrorUtc = issue.LastErrorUtc,
                    Status = issue.Status,
                    Priority = issue.MatchPriority,
                    UserName = assignedUser == null ? string.Empty : assignedUser.FullName,
                    Users = users.Items.ToSelectList(u => u.Id, u => "{0} {1}".FormatWith(u.FirstName, u.LastName), sortListBy: SortSelectListBy.Text, selected: u => u.Id == issue.UserId),
                    Statuses = issue.Status.ToSelectedList(Issue.ResourceManager, false, issue.Status.ToString()),
                    //Priorities = priorities,
                    UserId = issue.UserId,
                    ApplicationName = applications.Items.First(a => a.Id == issue.ApplicationId).Name,
                    ErrorLimitStatus = Issue.ResourceManager.GetString("ErrorLimitStatus_{0}".FormatWith(issue.LimitStatus)) ,
                    ProdProfRecords = issue.ProdProfRecords,
                    AlwaysNotify = issue.AlwaysNotify,
                    Reference = issue.Reference,
                    History = issue.History.OrderByDescending(h => h.DateAddedUtc).Select(h => 
                    {
                        var user = users.Items.FirstOrDefault(u => u.Id == h.UserId);
                        return new IssueHistoryItemViewModel
                        {
                            Message = h.Message,
                            DateAddedUtc = h.DateAddedUtc,
                            UserEmail = user != null ? user.Email : string.Empty,
                            Username = user != null ? user.FullName : string.Empty,
                            SystemMessage = h.SystemMessage,
                            Reference = h.Reference
                        };
                    }).ToList(),
                    TestIssue = issue.TestIssue,
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

            var result = _adjustRulesCommand.Invoke(new AdjustRulesRequest
            {
                IssueId = postModel.Id,
                ApplicationId = postModel.ApplicationId,
                Rules = postModel.Rules.Select(r => (IMatchRule)new PropertyMatchRule(r.ErrorProperty, r.StringOperator, r.Value)).ToList(),
                CurrentUser = Core.AppContext.CurrentUser,
                NewIssueName = postModel.UnmatchedIssueName,
                //NewPriority = postModel.UnmatchedIssuePriority,
                OriginalIssueName = postModel.IssueNameAfterUpdate,
                //OriginalPriority = postModel.MatchPriorityAfterUpdate
            });

            switch(result.Status)
            {
                case AdjustRulesStatus.IssueNotFound:
                    return RedirectToAction("notfound", new { FriendlyId = postModel.Id.GetFriendlyId() });
                case AdjustRulesStatus.Ok:
                    ConfirmationNotification(new MvcHtmlString("Rules adjusted successfully. Of {0} errors, {1} still match{2} and remain{3} attached to the issue.{4}".FormatWith(
                        result.ErrorsMatched + result.ErrorsNotMatched,
                        result.ErrorsNotMatched == 0 ? "all" : result.ErrorsMatched.ToString(),
                        result.ErrorsMatched == 1 ? "es": "",
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
                Comment = postModel.Comment,
                AssignedUserId = postModel.UserId
            });

            if (result.Status == UpdateIssueDetailsStatus.IssueNotFound)
            {
                return RedirectToAction("notfound", new { FriendlyId = postModel.IssueId.GetFriendlyId() });
            }

            ConfirmationNotification(Issue.DetailsUpdated);
            return RedirectToAction("index", new { id = postModel.IssueId, tab = IssueTab.Details.ToString() });
        }

        [HttpPost, ExportViewData]
        public ActionResult Purge(string issueId)
        {
            _purgeIssueCommand.Invoke(new PurgeIssueRequest
            {
                IssueId = issueId,
                CurrentUser = Core.AppContext.CurrentUser
            });

            ConfirmationNotification(Issue.IssuePurged);

            if(Request.IsAjaxRequest())
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

            var httpTask = new HttpClient().PostJsonAsync("{0}/api/errors".FormatWith(_configuration.ReceptionHttpEndpoint), request);
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

            ConfirmationNotification(Issue.DeleteIssueStatus_Ok.FormatWith(issueId));

            return RedirectToAction("index", "issues");
        }
    }
}
