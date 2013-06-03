using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Errordite.Core;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Encryption;
using Errordite.Core.Extensions;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Indexing;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Matching;
using Errordite.Core.Web;
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
    public class IssueController : ErrorditeController
	{
		private static readonly IEnumerable<SelectListItem> _frequencyHours = new []
		{
			new SelectListItem {Text = "Never", Value = "0"},
			new SelectListItem {Text = "Hourly", Value = new Duration(hours: 1).ToString()},
			new SelectListItem {Text = "Every 4 hours", Value = new Duration(hours: 4).ToString()},
			new SelectListItem {Text = "Daily", Value = new Duration(days: 1).ToString()},
			new SelectListItem {Text = "Weekly", Value = new Duration(weeks: 1).ToString()},
            new SelectListItem {Text = "Monthly", Value = new Duration(months: 1).ToString()},
            new SelectListItem {Text = "Every 3 months", Value = new Duration(months: 3).ToString()},
		};

        private readonly IGetIssueQuery _getIssueQuery;
        private readonly IAdjustRulesCommand _adjustRulesCommand;
        private readonly IGetApplicationErrorsQuery _getApplicationErrorsQuery;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IUpdateIssueDetailsCommand _updateIssueDetailsCommand;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IDeleteIssueErrorsCommand _deleteIssueErrorsCommand;
        private readonly IDeleteIssueCommand _deleteIssueCommand;
        private readonly IGetIssueReportDataQuery _getIssueReportDataQuery;
        private readonly IGetExtraDataKeysForIssueQuery _getExtraDataKeysForIssueQuery;
		private readonly IAddCommentCommand _addCommentCommand;
	    private readonly IEncryptor _encryptor;
		private readonly IGetOrganisationQuery _getOrganisationQuery;

        public IssueController(IGetIssueQuery getIssueQuery, 
            IAdjustRulesCommand adjustRulesCommand, 
            IGetApplicationErrorsQuery getApplicationErrorsQuery, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IUpdateIssueDetailsCommand updateIssueDetailsCommand, 
            ErrorditeConfiguration configuration, 
            IDeleteIssueErrorsCommand deleteIssueErrorsCommand, 
            IDeleteIssueCommand deleteIssueCommand,
            IGetIssueReportDataQuery getIssueReportDataQuery, 
            IGetExtraDataKeysForIssueQuery getExtraDataKeysForIssueQuery, 
			IAddCommentCommand addCommentCommand, 
			IEncryptor encryptor,
			IGetOrganisationQuery getOrganisationQuery)
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
            _getExtraDataKeysForIssueQuery = getExtraDataKeysForIssueQuery;
	        _addCommentCommand = addCommentCommand;
	        _encryptor = encryptor;
	        _getOrganisationQuery = getOrganisationQuery;
        }

		[
		ImportViewData,
		GenerateBreadcrumbs(BreadcrumbId.Issue, BreadcrumbId.Issues, WebConstants.CookieSettings.IssueSearchCookieKey)
		]
		public ActionResult Public(PublicIssuePostModel model)
		{
			var token = _encryptor.Decrypt(model.Token.Base64Decode()).Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries);

			if (token.Length == 2)
			{
				var organisation = _getOrganisationQuery.Invoke(new GetOrganisationRequest
				{
					OrganisationId = token[0]
				}).Organisation;

				if (organisation != null)
				{
					AppContext.CurrentUser.ActiveOrganisation = organisation;
					AppContext.CurrentUser.OrganisationId = organisation.Id;

					using (Core.Session.SwitchOrg(organisation))
					{
						model.Id = token[1];

						var viewModel = GetViewModel(model, new PageRequestWithSort(1, 10, CoreConstants.SortFields.TimestampUtc, sortDescending: true), true);

						if (viewModel == null)
						{
							Response.StatusCode = 404;
							return View("NotFound", new IssueNotFoundViewModel { Id = model.Id.GetFriendlyId() });
						}

						viewModel.ReadOnly = true;
						viewModel.Errors.ReadOnly = true;

						return View("index", viewModel);
					}
				}
			}

			ErrorNotification("Failed to determine issue from token");
			return Redirect(Url.Home());
		}

        [
		Authorize,
        ImportViewData, 
        GenerateBreadcrumbs(BreadcrumbId.Issue, BreadcrumbId.Issues, WebConstants.CookieSettings.IssueSearchCookieKey),
        PagingView(DefaultSort = CoreConstants.SortFields.TimestampUtc, DefaultSortDescending = true)
        ]
        public ActionResult Index(IssueErrorsPostModel postModel)
        {
            //this is a bit of a hack but the paging URL is generated based on the page URL so if we get
            //an ajax call just assume it's error page
            if (Request.IsAjaxRequest())
                return Errors(new ErrorCriteriaPostModel
                    {
                        Id = postModel.Id,
                    });

            var viewModel = GetViewModel(postModel, GetSinglePagingRequest());

            if(viewModel == null)
            {
                Response.StatusCode = 404;
                return View("NotFound", new IssueNotFoundViewModel { Id = postModel.Id.GetFriendlyId() });
            }

            return View(viewModel);
        }

        private IssueViewModel GetViewModel(IssueErrorsPostModel postModel, PageRequestWithSort paging, bool useSystemUser = false)
        {
            var issue = _getIssueQuery.Invoke(new GetIssueRequest
	        {
		        IssueId = postModel.Id, 
				CurrentUser = useSystemUser ? Errordite.Core.Domain.Organisation.User.System() : Core.AppContext.CurrentUser
	        }).Issue;

            if (issue == null)
                return null;

            var users = Core.GetUsers();
            var applications = Core.GetApplications();
            var assignedUser = users.Items.FirstOrDefault(u => u.Id == issue.UserId);

            //if the assigned user has been deleted, update it to the current user
            if (assignedUser == null)
            {
                var updateIssue = Core.Session.Raven.Load<Issue>(issue.Id);
                updateIssue.UserId = Core.AppContext.CurrentUser.Id;
                assignedUser = Core.AppContext.CurrentUser;
            }

            int ii = 0;

            var extraDataKeys = _getExtraDataKeysForIssueQuery.Invoke(new GetExtraDataKeysForIssueRequest
            {
                IssueId = issue.Id,
            }).Keys ?? new List<string>();

            var ruleViewModels = issue.Rules.OfType<PropertyMatchRule>().Select(r => new RuleViewModel
            {
                ErrorProperty = r.ErrorProperty,
                StringOperator = r.StringOperator,
                Value = r.Value,
                Index = ii++,
                Properties = _configuration.GetRuleProperties(r.ErrorProperty)
                                           .Union(extraDataKeys.Select(k => new SelectListItem
                                            {
                                                Selected = r.ErrorProperty == k,
                                                Text = k,
                                                Value = k
                                            })),
            }).ToList();

            var updateViewModel = new UpdateIssueViewModel
            {
                ApplicationId = issue.ApplicationId,
                Rules = ruleViewModels,
                Name = issue.Name,
                AdjustmentName = GetAdjustmentRejectsName(issue.Name),
                IssueId = issue.Id,
                Users = users.Items.ToSelectList(u => u.Id, u => "{0} {1}".FormatWith(u.FirstName, u.LastName), sortListBy: SortSelectListBy.Text, selected: u => u.Id == issue.UserId),
                Statuses = issue.Status.ToSelectedList(IssueResources.ResourceManager, false, issue.Status == IssueStatus.Unacknowledged ? IssueStatus.Acknowledged.ToString() : issue.Status.ToString()),
                UserId = issue.UserId,
                Status = issue.Status == IssueStatus.Unacknowledged ? IssueStatus.Acknowledged : issue.Status,
                NotifyFrequency = issue.NotifyFrequency,
                Reference = issue.Reference,
				NotificationFrequencies = _frequencyHours,
				Comment = null
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
                    IssueId = issue.Id,
                    Status = issue.Status,
                    NotifyFrequency = issue.NotifyFrequency,
                    Reference = issue.Reference
                },
                Errors = GetErrorsViewModel(postModel, paging, extraDataKeys),
                Update = updateViewModel,
                Tab = postModel.Tab,
				PublicUrl = "{0}/issue/public?token={1}".FormatWith(
					_configuration.SiteBaseUrl, 
					_encryptor.Encrypt("{0}|{1}".FormatWith(
						Core.AppContext.CurrentUser.ActiveOrganisation.FriendlyId,
						issue.FriendlyId)).Base64Encode())
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

		[Authorize, ImportViewData]
        public ActionResult NotFound(IssueNotFoundViewModel viewModel)
        {
            return View(viewModel);
        }

		[Authorize, PagingView(DefaultSort = Errordite.Core.CoreConstants.SortFields.TimestampUtc, DefaultSortDescending = true), ImportViewData]
        public ActionResult Errors(ErrorCriteriaPostModel postModel)
        {
            var paging = GetSinglePagingRequest();
            var model = GetErrorsViewModel(postModel, paging, _getExtraDataKeysForIssueQuery.Invoke(new GetExtraDataKeysForIssueRequest(){IssueId = postModel.Id}).Keys);
            return PartialView("Errors/ErrorItems", model);
        }

        private ErrorCriteriaViewModel GetErrorsViewModel(ErrorCriteriaPostModel postModel, PageRequestWithSort paging, List<string> extraDataKeys)
        {
            var request = new GetApplicationErrorsRequest
            {
                OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                IssueId = postModel.Id,
                Paging = paging,
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
                Errors = errors.Items.Select(e => new ErrorInstanceViewModel { Error = e, HideIssues = true, PropertiesEligibleForRules = extraDataKeys}).ToList(),
                HideIssues = true,
                Id = postModel.Id,
                Sort = paging.Sort,
                SortDescending = paging.SortDescending,
                
            };

            model.Paging.Tab = IssueTab.Details.ToString();
            return model;
        }

		[Authorize]
        public ActionResult History(string issueId)
        {
            var issueMemoizer = new LocalMemoizer<string, Issue>(id => _getIssueQuery.Invoke(new GetIssueRequest
	        {
		        CurrentUser = Core.AppContext.CurrentUser, IssueId = id
	        }).Issue);

			var history = Core.Session.Raven.Query<HistoryDocument, Core.Indexing.History>()
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
                        VerbalTime = h.DateAddedUtc.ToVerbalTimeSinceUtc(Core.AppContext.CurrentUser.ActiveOrganisation.TimezoneId),
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

		[Authorize, HttpPost, ValidateInput(false), ActionName("AdjustRules"), IfButtonClicked("WhatIf")]
        public ActionResult WhatIfAdjustRules(UpdateIssuePostModel postModel)
        {
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                    return new JsonErrorResult();

                return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.IssueId.GetFriendlyId(), tab = IssueTab.Rules.ToString() });
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

		[Authorize, HttpPost, ExportViewData, ValidateInput(false), IfButtonClicked("AdjustRules")]
        public ActionResult AdjustRules(UpdateIssuePostModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.IssueId.GetFriendlyId(), tab = IssueTab.Rules.ToString() }); 
            }

            try
            {
                var updateResult = _updateIssueDetailsCommand.Invoke(new UpdateIssueDetailsRequest
                {
                    IssueId = postModel.IssueId,
                    Status = postModel.Status,
                    Name = postModel.Name,
                    CurrentUser = Core.AppContext.CurrentUser,
                    NotifyFrequency = postModel.NotifyFrequency,
                    Reference = postModel.Reference,
                    AssignedUserId = postModel.UserId,
                    Comment = postModel.Comment,
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
                        ConfirmationNotification(new MvcHtmlString("Issue details updated successfully, of {0}, {1} still match{2} and remain{3} attached to the issue.{4}".FormatWith(
                            "error".Quantity(result.ErrorsMatched + result.ErrorsNotMatched),
                            result.ErrorsMatched == 1 ? "1" : result.ErrorsNotMatched == 0 ? "all" : result.ErrorsMatched.ToString(),
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
                    default:
                        return RedirectWithViewModel(postModel, "index", "Issue details were updated successfully, the rules for this issue were not changed.", false, new { id = postModel.IssueId.GetFriendlyId(), tab = IssueTab.Details.ToString() });
                }

                return RedirectToAction("index", new { id = result.IssueId.GetFriendlyId(), tab = IssueTab.Details.ToString() });
            }
            catch (ConcurrencyException)
            {
                ErrorNotification("A background process modified this issue at the same time as you requested to adjust the rules which resulted in a failure, please try again.");
				return RedirectToAction("index", new { id = postModel.IssueId.GetFriendlyId(), tab = IssueTab.Details.ToString() });
            }
        }

		[Authorize, HttpPost, ExportViewData]
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
            catch (ConcurrencyException)
            {
                ErrorNotification("A background process modified this issues data at the same time as you requested to delete the errors, please try again.");
            }

            if (Request.IsAjaxRequest())
                return new JsonSuccessResult();

            return RedirectToAction("index", new { id = issueId.GetFriendlyId(), tab = IssueTab.History.ToString() });
        }

		[Authorize, ActionName("Reprocess"), IfButtonClicked("WhatIf"), HttpPost]
        public ActionResult WhatIfReprocess(string issueId)
        {
            var request = new ReprocessIssueErrorsRequest
            {
                IssueId = issueId,
                CurrentUser = Core.AppContext.CurrentUser,
                WhatIf = true,
            };

            var httpTask = Core.Session.ReceiveHttpClient.PostJsonAsync("ReprocessIssueErrors", request);
            httpTask.Wait();

            if (!httpTask.Result.IsSuccessStatusCode)
            {
                Response.StatusCode = 500;
                return Content("error");
            }

            var contentTask = httpTask.Result.Content.ReadAsStringAsync();
            contentTask.Wait();

            var response = JsonConvert.DeserializeObject<ReprocessIssueErrorsResponse>(contentTask.Result);

            if (response.Status == ReprocessIssueErrorsStatus.NotAuthorised)
            {
                throw new ErrorditeAuthorisationException(new Issue { Id = issueId }, Core.AppContext.CurrentUser);
            }
                
            return Content(response.GetMessage(Issue.GetId(issueId)).ToString());
        }

		[Authorize, HttpPost, ExportViewData]
		public ActionResult AddComment(AddCommentViewModel postModel)
		{
			if (!ModelState.IsValid)
			{
				return RedirectWithViewModel(postModel, "index", routeValues: new { id = postModel.IssueId.GetFriendlyId(), tab = IssueTab.Details.ToString() });
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

			ConfirmationNotification("Comment was added to this issue successfully");
			return RedirectToAction("index", new { id = postModel.IssueId.GetFriendlyId(), tab = IssueTab.History.ToString() });
		}

		[Authorize, HttpPost, ExportViewData, ActionName("Reprocess"), IfButtonClicked("Reprocess")]
        public ActionResult Reprocess(string issueId)
        {
            var request = new ReprocessIssueErrorsRequest
            {
                IssueId = issueId,
                CurrentUser = Core.AppContext.CurrentUser
            };

            var httpTask = Core.Session.ReceiveHttpClient.PostJsonAsync("ReprocessIssueErrors", request);
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
                    throw new ErrorditeAuthorisationException(new Issue { Id = issueId }, Core.AppContext.CurrentUser);
                }

				//getting consistent concurrency exceptions when this is executed in the service, so moved it to here
				Core.Session.Raven.Store(new IssueHistory
				{
					DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId),
					UserId = request.CurrentUser.Id,
					Type = HistoryItemType.ErrorsReprocessed,
					ReprocessingResult = response.AttachedIssueIds,
					IssueId = Issue.GetId(issueId),
					ApplicationId = response.ApplicationId,
					SystemMessage = true
				});

                ConfirmationNotification(response.GetMessage(Errordite.Core.Domain.Error.Issue.GetId(issueId)));
            }

            return RedirectToAction("index", new { id = issueId.GetFriendlyId(), tab = IssueTab.Details.ToString() });
        }

		[Authorize, HttpPost, ExportViewData]
        public ActionResult Delete(string issueId)
        {
            _deleteIssueCommand.Invoke(new DeleteIssueRequest
            {
                IssueId = issueId,
                CurrentUser = Core.AppContext.CurrentUser
            });

            ConfirmationNotification(IssueResources.DeleteIssueStatus_Ok.FormatWith(IdHelper.GetFriendlyId(issueId)));

            return RedirectToAction("index", "issues");
        }
    }
}
