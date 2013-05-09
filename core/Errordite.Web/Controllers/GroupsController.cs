using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Errordite.Core.Extensions;
using Errordite.Core.Paging;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Groups.Commands;
using Errordite.Core.Groups.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Extensions;
using Errordite.Web.Models.Groups;
using Errordite.Web.Models.Navigation;

namespace Errordite.Web.Controllers
{
	[Authorize, RoleAuthorize(UserRole.Administrator), ValidateSubscriptionActionFilter]
    public class GroupsController : ErrorditeController
    {
        private readonly IGetGroupQuery _getGroupQuery;
        private readonly IEditGroupCommand _editGroupCommand;
        private readonly IAddGroupCommand _addGroupCommand;
        private readonly IDeleteGroupCommand _deleteGroupCommand;
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;

        public GroupsController(IAddGroupCommand addGroupCommand,
            IEditGroupCommand editGroupCommand,
            IDeleteGroupCommand deleteGroupCommand, 
            IPagingViewModelGenerator pagingViewModelGenerator, 
            IGetGroupQuery getGroupQuery)
        {
            _addGroupCommand = addGroupCommand;
            _editGroupCommand = editGroupCommand;
            _deleteGroupCommand = deleteGroupCommand;
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _getGroupQuery = getGroupQuery;
        }

        [PagingView, HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.Groups)]
        public ActionResult Index()
        {
            var pagingRequest = GetSinglePagingRequest();
            var groups = Core.GetGroups(pagingRequest);

            var groupsViewModel = new GroupsViewModel
            {
                Groups = groups.Items.Select(Mapper.Map<Group, GroupViewModel>).ToList(),
                Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, groups.PagingStatus, pagingRequest)
            };

            return View(groupsViewModel);
        }

        [HttpGet, ImportViewData, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.AddGroup)]
        public ActionResult Add()
        {
            var users = Core.GetUsers();

            var postedViewModel = ViewData.Model == null ? null : (AddGroupViewModel)ViewData.Model;

            var viewModel = new AddGroupViewModel
            {
                Users = users.Items.Select(u => new GroupMemberViewModel
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Selected = postedViewModel != null && postedViewModel.Users.Any(user => u.Id == user.Id)
                })
            };

            return View(viewModel);
        }

        [HttpPost, ExportViewData]
        public ActionResult Add(AddGroupViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "add");
            }

            var result = _addGroupCommand.Invoke(new AddGroupRequest
            {
                Name = viewModel.Name,
                Organisation = Core.AppContext.CurrentUser.ActiveOrganisation,
                Users = viewModel.Users.Where(user => user.Selected).Select(user => user.Id).ToList()
            });

            if (result.Status != AddGroupStatus.Ok)
            {
                return RedirectWithViewModel(viewModel, "add", result.Status.MapToResource(Resources.Account.ResourceManager));
            }

            return Redirect(Url.Groups());
        }

        [HttpPost, ExportViewData]
        public ActionResult Edit(EditGroupViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                return RedirectWithViewModel(viewModel, "edit");
            }

            var result = _editGroupCommand.Invoke(new EditGroupRequest
            {
                Name = viewModel.Name,
                GroupId = viewModel.Id,
                CurrentUser = Core.AppContext.CurrentUser,
                Users = viewModel.Users.Where(user => user.Selected).Select(user => user.Id).ToList()
            });

            if(result.Status != EditGroupStatus.Ok)
            {
                return RedirectWithViewModel(viewModel, "edit", result.Status.MapToResource(Resources.Account.ResourceManager));
            }

            ConfirmationNotification(Resources.Account.Group_Updated);
            return Redirect(Url.Groups());
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.EditGroup)]
        public ActionResult Edit(string groupId)
        {
            var group = _getGroupQuery.Invoke(new GetGroupRequest
            {
                GroupId = groupId,
                CurrentUser = Core.AppContext.CurrentUser
            }).Group;

            var users = Core.GetUsers();
            var postedViewModel = ViewData.Model == null ? null : (EditGroupViewModel)ViewData.Model;

            var viewModel = new EditGroupViewModel
            {
                Name = group.Name,
                Id = group.FriendlyId,
                Users = users.Items.Select(u => new GroupMemberViewModel
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Selected = postedViewModel == null ? u.GroupIds.Any(id => id == group.Id) : postedViewModel.Users.Any(user => u.Id == user.Id)
                })
            };

            return View(viewModel);
        }

        [HttpPost, ExportViewData]
        public ActionResult Delete(string groupId)
        {
            var response = _deleteGroupCommand.Invoke(new DeleteGroupRequest
            {
                GroupId = groupId,
                CurrentUser = Core.AppContext.CurrentUser
            });

            if (response.Status != DeleteGroupStatus.Ok)
                ErrorNotification(response.Status.MapToResource(Resources.Account.ResourceManager));
            else
                ConfirmationNotification(Resources.Account.DeleteGroupStatus_Ok);

            return Redirect(Url.Groups());
        }
    }
}
