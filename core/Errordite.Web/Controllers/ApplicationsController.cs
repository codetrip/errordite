using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using AutoMapper;
using Errordite.Core.Extensions;
using Errordite.Core.Paging;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Matching;
using Errordite.Core.Receive.Commands;
using Errordite.Core.Web;
using Errordite.Web.ActionFilters;
using Errordite.Web.Extensions;
using System.Linq;
using Errordite.Web.Models.Applications;
using Errordite.Web.Models.Groups;
using Errordite.Web.Models.Navigation;
using Application = Errordite.Core.Domain.Organisation.Application;

namespace Errordite.Web.Controllers
{
	[Authorize, RoleAuthorize(UserRole.Administrator), ValidateSubscriptionActionFilter]
    public class ApplicationsController : ErrorditeController
    {
        private readonly IAddApplicationCommand _addApplicationCommand;
        private readonly IGetApplicationQuery _getApplicationQuery;
        private readonly IEditApplicationCommand _editApplicationCommand;
        private readonly IDeleteApplicationCommand _deleteApplicationCommand;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IMatchRuleFactoryFactory _matchRuleFactoryFactory;

        public ApplicationsController(IAddApplicationCommand addApplicationCommand, 
            IGetApplicationQuery getApplicationQuery, 
            IEditApplicationCommand editApplicationCommand, 
            IDeleteApplicationCommand deleteApplicationCommand, 
            IPagingViewModelGenerator pagingViewModelGenerator,
            IMatchRuleFactoryFactory matchRuleFactoryFactory)
        {
            _addApplicationCommand = addApplicationCommand;
            _getApplicationQuery = getApplicationQuery;
            _editApplicationCommand = editApplicationCommand;
            _deleteApplicationCommand = deleteApplicationCommand;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _matchRuleFactoryFactory = matchRuleFactoryFactory;
        }

        [HttpGet, ImportViewData, PagingView, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.Applications)]
        public ActionResult Index()
        {
            var pagingRequest = GetSinglePagingRequest();
            var applications = Core.GetApplications(pagingRequest);

            if (applications.Items == null || applications.Items.Count == 0)
            {
                ErrorNotification(Resources.Application.No_Applications);
                return Redirect(Url.AddApplication());
            }

            var applicationsViewModel = new ApplicationsViewModel
            {
                Applications = applications.Items.Select(Mapper.Map<Application, ApplicationViewModel>).ToList(),
                Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, applications.PagingStatus, pagingRequest)
            };

            var users = Core.GetUsers();

            foreach (var application in applicationsViewModel.Applications)
            {
                var user = users.Items.FirstOrDefault(u => u.Id == application.DefaultUserId);

				//if the default user has been deleted, update it here to the current user
				if (user == null)
				{
					var app = Core.Session.Raven.Load<Application>(application.Id);
					app.DefaultUserId = Core.AppContext.CurrentUser.Id;
					user = Core.AppContext.CurrentUser;
				}

                application.RuleMatchFactory = _matchRuleFactoryFactory.Create(application.RuleMatchFactory).Name;
                application.DefaultUser = "{0} {1}".FormatWith(user.FirstName, user.LastName);
            } 

            return View(applicationsViewModel);
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.AddApplication)]
        public ActionResult Add(bool? newOrganisation)
        {
            var applications = Core.GetApplications();
            var groups = Core.GetGroups();

            if (applications.PagingStatus.TotalItems >= Core.AppContext.CurrentUser.ActiveOrganisation.PaymentPlan.MaximumApplications)
            {
                SetNotification(AddApplicationStatus.PlanThresholdReached, Resources.Application.ResourceManager);
                return RedirectToAction("index", "subscription");
            }

            var viewModel = ViewData.Model == null ? new AddApplicationViewModel{Active = true} : (AddApplicationViewModel)ViewData.Model;

            viewModel.ErrorConfigurations = _matchRuleFactoryFactory
                .Create()
                .ToSelectList(f => f.Id, f => f.Description, f => f.Id == "1", sortListBy: SortSelectListBy.Text);

            viewModel.Users = Core.GetUsers()
                .Items
                .ToSelectList(u => u.FriendlyId, u => "{0} {1}".FormatWith(u.FirstName, u.LastName), sortListBy: SortSelectListBy.Text);

            if (viewModel.NotificationGroups == null || !viewModel.NotificationGroups.Any())
            {
                viewModel.NotificationGroups = groups.Items.Select(g => new GroupViewModel {Id = g.Id, Name = g.Name}).ToList();
            }

            if (newOrganisation.HasValue && newOrganisation.Value)
                viewModel.NewOrganisation = true;

            viewModel.Version = "1.0.0.0";

            return View(viewModel);
        }

        [HttpPost, ExportViewData]
        public ActionResult Add(AddApplicationPostModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(Mapper.Map<AddApplicationPostModel, AddApplicationViewModel>(viewModel), "add");
            }

            var response = _addApplicationCommand.Invoke(new AddApplicationRequest
            {
                Name = viewModel.Name,
                IsActive = viewModel.Active,
                CurrentUser = Core.AppContext.CurrentUser,
                MatchRuleFactoryId = new MethodAndTypeMatchRuleFactory().Id,
                UserId = viewModel.UserId,
                HipChatAuthToken = viewModel.HipChatAuthToken,
                HipChatRoomId = viewModel.HipChatRoomId,
                NotificationGroups = viewModel.NotificationGroups.Where(n => n.Selected).Select(g => g.Id).ToList(),
                Version = viewModel.Version
            });

            if (response.Status != AddApplicationStatus.Ok)
            {
                return RedirectWithViewModel(Mapper.Map<AddApplicationPostModel, AddApplicationViewModel>(viewModel), "add", response.Status.MapToResource(Resources.Application.ResourceManager));
            }

            ConfirmationNotification("Application '{0}' added.".FormatWith(viewModel.Name));
            return Redirect(Url.Applications());
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.EditApplication)]
        public ActionResult Edit(string applicationId)
        {
            var viewModel = ViewData.Model == null ? null : (EditApplicationViewModel)ViewData.Model;

            if(viewModel == null)
            {
                var groups = Core.GetGroups();
                var application = _getApplicationQuery.Invoke(new GetApplicationRequest
                {
                    ApplicationId = applicationId,
                    CurrentUser = Core.AppContext.CurrentUser,
                    OrganisationId = Core.AppContext.CurrentUser.OrganisationId
                }).Application;

                viewModel = Mapper.Map<Application, EditApplicationViewModel>(application);
                viewModel.NotificationGroups = groups.Items.Select(g => new GroupViewModel
                {
                    Id = g.Id, 
                    Name = g.Name, 
                    Selected = application.NotificationGroups.Any(n => n == g.Id)
                }).ToList();
            }

            viewModel.ErrorConfigurations = _matchRuleFactoryFactory
                .Create()
                .ToSelectList(f => f.Id, f => f.Description, f => f.Id == viewModel.MatchRuleFactoryId, sortListBy: SortSelectListBy.Text);

            viewModel.Users = Core.GetUsers()
                .Items
                .ToSelectList(u => u.FriendlyId, u => "{0} {1}".FormatWith(u.FirstName, u.LastName), u => u.FriendlyId == viewModel.UserId, sortListBy: SortSelectListBy.Text);

            return View(viewModel);
        }

        [HttpPost, ExportViewData]
        public ActionResult Edit(EditApplicationPostModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(Mapper.Map<EditApplicationPostModel, EditApplicationViewModel>(viewModel), "edit");
            }

            var response = _editApplicationCommand.Invoke(new EditApplicationRequest
            {
                Name = viewModel.Name,
                IsActive = viewModel.IsActive,
                CurrentUser = Core.AppContext.CurrentUser,
                ApplicationId = viewModel.Id,
                MatchRuleFactoryId = viewModel.MatchRuleFactoryId,
                HipChatAuthToken = viewModel.HipChatAuthToken,
                HipChatRoomId = viewModel.HipChatRoomId,
                UserId = Errordite.Core.Domain.Organisation.User.GetId(viewModel.UserId),
                NotificationGroups = viewModel.NotificationGroups.Where(n => n.Selected).Select(g => g.Id).ToList(),
                Version = viewModel.Version,
            });

            if (response.Status != EditApplicationStatus.Ok)
            {
                return RedirectWithViewModel(Mapper.Map<EditApplicationPostModel, EditApplicationViewModel>(viewModel), 
                    "edit", 
                    response.Status.MapToResource(Resources.Application.ResourceManager),
                    routeValues: new { applicationId = viewModel.Id });
            }

            ConfirmationNotification(Resources.Application.EditApplicationStatus_Ok.FormatWith(viewModel.Name));
            return Redirect(Url.Applications());
        }

        [HttpPost, ExportViewData]
        public ActionResult Delete(string applicationId)
        {
            var response = _deleteApplicationCommand.Invoke(new DeleteApplicationRequest
            {
                ApplicationId = applicationId,
                CurrentUser = Core.AppContext.CurrentUser
            });

            if (response.Status != DeleteApplicationStatus.Ok)
                ErrorNotification(response.Status.MapToResource(Resources.Application.ResourceManager));
            else
                ConfirmationNotification(Resources.Application.DeleteApplicationStatus_Ok);

            return Redirect(Url.Applications());
        }

        [ExportViewData]
        public ActionResult GenerateError(string applicationid, string returnUrl)
        {
            var application = _getApplicationQuery.Invoke(new GetApplicationRequest
            {
                CurrentUser = AppContext.CurrentUser,
                ApplicationId = applicationid,
                OrganisationId = AppContext.CurrentUser.OrganisationId,
            }).Application;

            if (application == null)
            {
                return HttpNotFound();
            }

            var task = Core.Session.ReceiveHttpClient.PostJsonAsync("error", new ReceiveErrorRequest
            {
                Error = new Error
                {
                    ExceptionInfos = new[]{ new ExceptionInfo
                    {
                        Type = "Errordite.TestException",
                        Message = "Test exception",
                        MethodName = "TestMethod",
                        Module = "Test.Module",
                        StackTrace = " at Errordite.TestComponent.TestMethod(TestParam param) in E:\\SourceCode\\Errordite.Test\\TestComponent.cs:line 70\n   at System.Web.Mvc.ActionMethodDispatcher.Execute(ControllerBase controller, Object[] parameters)",
                        ExtraData = new Dictionary<string, string>
                        {
                            {"Url", "http://myapp.com/test?foo=1234"}
                        },
                    }}.ToArray(),
                    MachineName = "Test Machine Name",
                    TimestampUtc = DateTime.UtcNow,
                    ApplicationId = application.Id,
                    OrganisationId = application.OrganisationId,
                    TestError = true,
                    Version = application.Version
                },
                ApplicationId = application.Id,
                Organisation = AppContext.CurrentUser.ActiveOrganisation,
            }).ContinueWith(t =>
                {
                    t.Result.EnsureSuccessStatusCode();
                    return t.Result.Content.ReadAsAsync<ReceiveErrorResponse>().Result;
                });

            var receiveErrorResponse = task.Result;

            ConfirmationNotification(new MvcHtmlString("Test error generated - attached to issue <a href='{0}'>{1}</a>"
                .FormatWith(Url.Issue(IdHelper.GetFriendlyId(receiveErrorResponse.IssueId)), IdHelper.GetFriendlyId(receiveErrorResponse.IssueId))));

            return Redirect(returnUrl);
        }
    }
}
