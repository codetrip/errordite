using System;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using CodeTrip.Core.Paging;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Matching;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Extensions;
using CodeTrip.Core.Extensions;
using Errordite.Web.Models.Issues;
using Errordite.Web.Models.Navigation;
using Application = Errordite.Core.Domain.Organisation.Application;

namespace Errordite.Web.Controllers
{
    [Authorize]
    public class IssuesController : ErrorditeController
    {
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IGetApplicationIssuesQuery _getApplicationIssuesQuery;
        private readonly IMatchRuleFactoryFactory _matchRuleFactoryFactory;
        private readonly IAddIssueCommand _addIssueCommand;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IMergeIssuesCommand _mergeIssuesCommand;
        private readonly IGetIssueQuery _getIssueQuery;
        private readonly IBatchStatusUpdateCommand _batchStatusUpdateCommand;
        private readonly IBatchDeleteIssuesCommand _batchDeleteIssuesCommand;

        public IssuesController(IPagingViewModelGenerator pagingViewModelGenerator, 
            IGetApplicationIssuesQuery getApplicationIssuesQuery, 
            IMatchRuleFactoryFactory matchRuleFactoryFactory, 
            ErrorditeConfiguration configuration, 
            IAddIssueCommand addIssueCommand, 
            IGetIssueQuery getIssueQuery, 
            IMergeIssuesCommand mergeIssuesCommand, 
            IBatchStatusUpdateCommand batchStatusUpdateCommand, 
            IBatchDeleteIssuesCommand batchDeleteIssuesCommand)
        {
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _getApplicationIssuesQuery = getApplicationIssuesQuery;
            _matchRuleFactoryFactory = matchRuleFactoryFactory;
            _configuration = configuration;
            _addIssueCommand = addIssueCommand;
            _getIssueQuery = getIssueQuery;
            _mergeIssuesCommand = mergeIssuesCommand;
            _batchStatusUpdateCommand = batchStatusUpdateCommand;
            _batchDeleteIssuesCommand = batchDeleteIssuesCommand;
        }

        [
        PagingView(DefaultSort = CoreConstants.SortFields.LastErrorUtc, DefaultSortDescending = true), 
        ExportViewData, 
        ImportViewData,
        StoreQueryInCookie(WebConstants.CookieSettings.IssueSearchCookieKey),
        GenerateBreadcrumbs(BreadcrumbId.Issues)
        ]
        public ActionResult Index(IssueCriteriaPostModel postModel)
        {
            var viewModel = new IssueCriteriaViewModel();
            var applications = Core.GetApplications();

            if (applications.PagingStatus.TotalItems > 0)
            {
                var pagingRequest = GetSinglePagingRequest();

                if(postModel.Status == null || postModel.Status.Length == 0)
                {
                    postModel.Status = Enum.GetNames(typeof (IssueStatus)).Where(s => s != IssueStatus.Solved.ToString()).ToArray();
                }
                else if(postModel.Status.Length == 1 && postModel.Status[0].Contains(","))
                {
                    //this is a fix up for when the url gets changed to contain the statuses in a single query parameter
                    postModel.Status = postModel.Status[0].Split(',');
                }

                var request = new GetApplicationIssuesRequest
                {
                    ApplicationId = postModel.ApplicationId,
                    Paging = pagingRequest,
                    AssignedTo = postModel.AssignedTo,
                    StartDate = postModel.StartDate,
                    EndDate = postModel.EndDate,
                    Status = postModel.Status,
                    Name = postModel.Name,
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId,
                    UserTimezoneId = AppContext.CurrentUser.EffectiveTimezoneId(),
                };

                var issues = _getApplicationIssuesQuery.Invoke(request).Issues;
                var users = Core.GetUsers();

                viewModel.AssignedTo = postModel.AssignedTo;
                viewModel.StartDate = postModel.StartDate;
                viewModel.EndDate = postModel.EndDate;
                viewModel.Status = postModel.Status;
                viewModel.Name = postModel.Name;
                viewModel.Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, issues.PagingStatus, pagingRequest);
                viewModel.Users = users.Items.ToSelectList(u => u.FriendlyId, u => u.FullName, u => u.FriendlyId == postModel.AssignedTo);
                viewModel.Statuses = Enum.GetNames(typeof(IssueStatus)).ToSelectList(s => s, s => s, s => s.IsIn(postModel.Status));
                viewModel.Issues = IssueItemViewModel.FromIssues(issues.Items, applications.Items, users.Items);
                viewModel.ApplicationName = postModel.ApplicationId.IsNullOrEmpty() ? Resources.Application.AllApplications : applications.Items.First(a => a.Id == Application.GetId(postModel.ApplicationId)).Name;
                viewModel.ApplicationId = postModel.ApplicationId;
                viewModel.Applications = applications.Items.ToSelectList(a => a.FriendlyId, a => a.Name, u => u.FriendlyId == postModel.ApplicationId, Resources.Shared.Application, string.Empty, SortSelectListBy.Text);
            }
            else
            {
                ErrorNotification(Resources.Application.No_Applications);
                return Redirect(Url.AddApplication());
            }

            return View(viewModel);
        }

        [HttpGet, GenerateBreadcrumbs(BreadcrumbId.AddIssue, BreadcrumbId.Issues, WebConstants.CookieSettings.IssueSearchCookieKey)]
        public ActionResult Add()
        {
            var viewModel = ViewData.Model == null ? new AddIssueViewModel() : (AddIssueViewModel)ViewData.Model;

            var users = Core.GetUsers();
            var applications = Core.GetApplications();

            if(viewModel.Rules == null)
            {
                var rules = _matchRuleFactoryFactory.Create(new MethodAndTypeMatchRuleFactory().Id).CreateEmpty();

                int ii = 0;
                var ruleViewModels = rules.OfType<PropertyMatchRule>().Select(r => new RuleViewModel
                {
                    ErrorProperty = r.ErrorProperty,
                    StringOperator = r.StringOperator,
                    Value = r.Value,
                    Index = ii++,
                    Properties = _configuration.GetRuleProperties(r.ErrorProperty)
                }).ToList();

                viewModel.Rules = ruleViewModels;
            }

            viewModel.Priorities = MatchPriority.Medium.ToSelectedList(Resources.IssueResources.ResourceManager, false, MatchPriority.Medium.ToString());
            viewModel.Users = users.Items.ToSelectList(u => u.FriendlyId, u => "{0} {1}".FormatWith(u.FirstName, u.LastName), sortListBy: SortSelectListBy.Text);
            viewModel.Statuses = IssueStatus.Acknowledged.ToSelectedList(Resources.IssueResources.ResourceManager, false, IssueStatus.Acknowledged.ToString());
            viewModel.Applications = applications.Items.ToSelectList(a => a.FriendlyId, a => a.Name, sortListBy: SortSelectListBy.Text);
            
            return View(viewModel);
        }

        [HttpPost, ExportViewData, ValidateInput(false)]
        public ActionResult Add(AddIssuePostModel postModel)
        {
            if(!ModelState.IsValid)
            {
                return RedirectWithViewModel(Mapper.Map<AddIssuePostModel, AddIssueViewModel>(postModel), "add");
            }

            var result = _addIssueCommand.Invoke(new AddIssueRequest
            {
                ApplicationId = postModel.ApplicationId,
                Rules = postModel.Rules.Select(r => (IMatchRule)new PropertyMatchRule(r.ErrorProperty, r.StringOperator, r.Value)).ToList(),
                UserId = postModel.UserId,
                CurrentUser = Core.AppContext.CurrentUser,
                Name = postModel.Name,
                Status = postModel.Status,
                Priority = postModel.Priority
            });

            if (result.Status == AddIssueStatus.SameRulesExist)
                ErrorNotification(Resources.IssueResources.SameRulesExist.FormatWith(Url.Issue(result.IssueId)));
            else
                ConfirmationNotification(Resources.IssueResources.AddedSuccessfully);

            return Redirect(Url.Issue(result.IssueId));
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.MergeIssues, BreadcrumbId.Issues, WebConstants.CookieSettings.IssueSearchCookieKey)]
        public ActionResult Merge(MergeIssuesViewModel viewModel)
        {
            var leftIssue = _getIssueQuery.Invoke(new GetIssueRequest
            {
                IssueId = viewModel.LeftIssueId,
                CurrentUser = Core.AppContext.CurrentUser
            }).Issue;

            var rightIssue = _getIssueQuery.Invoke(new GetIssueRequest
            {
                IssueId = viewModel.RightIssueId,
                CurrentUser = Core.AppContext.CurrentUser
            }).Issue;

            viewModel.RightIssueName = rightIssue.Name;
            viewModel.RightIssueStatus = Resources.IssueResources.ResourceManager.GetString("IssueStatus_{0}".FormatWith(rightIssue.Status));
            viewModel.RightIssuePriority = Resources.IssueResources.ResourceManager.GetString("IssuePriority_{0}".FormatWith(rightIssue.MatchPriority));

            viewModel.LeftIssueName = leftIssue.Name;
            viewModel.LeftIssueStatus = Resources.IssueResources.ResourceManager.GetString("IssueStatus_{0}".FormatWith(leftIssue.Status));
            viewModel.LeftIssuePriority = Resources.IssueResources.ResourceManager.GetString("IssuePriority_{0}".FormatWith(leftIssue.MatchPriority));

            return View(viewModel);
        }

        [HttpPost, ExportViewData]
        public ActionResult Merge(MergeIssuesPostModel viewModel)
        {
            var response = _mergeIssuesCommand.Invoke(new MergeIssuesRequest
            {
                MergeFromIssueId = viewModel.MergeFromId,
                MergeToIssueId = viewModel.MergeToId,
                CurrentUser = Core.AppContext.CurrentUser
            });

            SetNotification(response.Status, Resources.IssueResources.ResourceManager);

            return Redirect(Url.Issue(viewModel.MergeToId));
        }

        [HttpPost, ExportViewData]
        public ActionResult BatchIssueAction(BatchIssueActionForm actionForm)
        {
            switch(actionForm.Action)
            {
                case Models.Issues.BatchIssueAction.StatusUpdate:
                    {
                        _batchStatusUpdateCommand.Invoke(new BatchStatusUpdateRequest
                        {
                            CurrentUser = AppContext.CurrentUser,
                            IssueIds = actionForm.IssueIds,
                            Status = actionForm.Status,
                            Comment = actionForm.Comment,
                            AssignToUserId = actionForm.AssignToUser.IsNullOrEmpty() ? null : Errordite.Core.Domain.Organisation.User.GetId(actionForm.AssignToUser),
                        });

                        ConfirmationNotification("Successfully updated {0} issues".FormatWith(actionForm.IssueIds.Count));
                    }
                    break;
                case Models.Issues.BatchIssueAction.Delete:
                    {
                        _batchDeleteIssuesCommand.Invoke(new BatchDeleteIssuesRequest
                        {
                            CurrentUser = AppContext.CurrentUser,
                            IssueIds = actionForm.IssueIds
                        });

                        ConfirmationNotification("Successfully deleted {0} issues".FormatWith(actionForm.IssueIds.Count));
                    }
                    break;
            }

            return new JsonSuccessResult();
        }
    }
}
