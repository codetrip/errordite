using System;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Matching;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.ActionResults;
using Errordite.Web.Areas.System.Models.Organisations;
using Errordite.Web.Controllers;
using Errordite.Web.Models.Admin;
using Errordite.Web.Models.Applications;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Organisation;
using Errordite.Web.Models.Users;

namespace Errordite.Web.Areas.System.Controllers
{
    [Authorize, RoleAuthorize]
    public class OrganisationsController : AdminControllerBase
    {
        private readonly IPagingViewModelGenerator _pagingViewModelGenerator;
        private readonly IGetOrganisationsQuery _getOrganisationsQuery;
        private readonly IGetUsersQuery _getUsersQuery;
        private readonly ISuspendOrganisationCommand _suspendOrganisationCommand;
        private readonly IActivateOrganisationCommand _activateOrganisationCommand;
        private readonly IMatchRuleFactoryFactory _matchRuleFactoryFactory;
        private readonly IGetApplicationsQuery _getApplicationsQuery;
        private readonly IDeleteOrganisationCommand _deleteOrganisationCommand;

        public OrganisationsController(IPagingViewModelGenerator pagingViewModelGenerator, 
            IGetOrganisationsQuery getOrganisationsQuery, 
            IGetUsersQuery getUsersQuery, 
            ISuspendOrganisationCommand suspendOrganisationCommand, 
            IActivateOrganisationCommand activateOrganisationCommand, 
            IMatchRuleFactoryFactory matchRuleFactoryFactory, 
            IGetApplicationsQuery getApplicationsQuery, 
            IDeleteOrganisationCommand deleteOrganisationCommand, IGetOrganisationQuery getOrganisationQuery, IAppSession appSession)
        {
            _pagingViewModelGenerator = pagingViewModelGenerator;
            _getOrganisationsQuery = getOrganisationsQuery;
            _getUsersQuery = getUsersQuery;
            _suspendOrganisationCommand = suspendOrganisationCommand;
            _activateOrganisationCommand = activateOrganisationCommand;
            _matchRuleFactoryFactory = matchRuleFactoryFactory;
            _getApplicationsQuery = getApplicationsQuery;
            _deleteOrganisationCommand = deleteOrganisationCommand;
        }

        [HttpGet, ImportViewData, PagingView, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.AdminOrganisations)]
        public ActionResult Index()
        {
            var pagingRequest = GetSinglePagingRequest();

            var organisations = _getOrganisationsQuery.Invoke(new GetOrganisationsRequest
            {
                Paging = pagingRequest
            }).Organisations;

            return View(new OrganisationsViewModel
            {
                Organisations = organisations,
                Paging = _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, organisations.PagingStatus, pagingRequest)
            });
        }

        [HttpPost, ExportViewData]
        public ActionResult SuspendOrganisation(SuspendOrganisationViewModel viewModel)
        {
            var response = _suspendOrganisationCommand.Invoke(new SuspendOrganisationRequest
            {
                Message = viewModel.Message,
                OrganisationId = viewModel.OrganisationId,
                Reason = viewModel.Reason
            });

            ConfirmationNotification(response.Status.MapToResource(Resources.Admin.ResourceManager));

            return new JsonSuccessResult();
        }

        [HttpPost, ExportViewData]
        public ActionResult ActivateOrganisation(string organisationId)
        {
            var response = _activateOrganisationCommand.Invoke(new ActivateOrganisationRequest
            {
                OrganisationId = organisationId,
            });

            ConfirmationNotification(response.Status.MapToResource(Resources.Admin.ResourceManager));

            return RedirectToAction("index");
        }

        [HttpPost, ExportViewData]
        public ActionResult DeleteOrganisation(string organisationId)
        {
            _deleteOrganisationCommand.Invoke(new DeleteOrganisationRequest
            {
                OrganisationId = organisationId
            });

            ConfirmationNotification("Organisation deleted successfully.");

            return RedirectToAction("index");
        }

        [HttpGet, ImportViewData, PagingView, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.AdminUsers)]
        public ActionResult Users(string organisationId)
        {
            using (SwitchOrgScope(organisationId))
            {
                var pagingRequest = GetSinglePagingRequest();

                var users = _getUsersQuery.Invoke(new GetUsersRequest
                    {
                        OrganisationId = organisationId,
                        Paging = pagingRequest
                    }).Users;

                var usersViewModel = new UsersViewModel
                    {
                        Users = users.Items.Select(Mapper.Map<User, UserViewModel>).ToList(),
                        Paging =
                            _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId, users.PagingStatus,
                                                               pagingRequest),
                        EnableImpersonation = true,
                        OrganisationId = organisationId
                    };

                return View(usersViewModel);
            }
        }

        [HttpGet, ImportViewData, PagingView, ExportViewData, GenerateBreadcrumbs(BreadcrumbId.AdminApplications)]
        public ActionResult Applications(string organisationId)
        {
            using (SwitchOrgScope(organisationId))
            {
                var pagingRequest = GetSinglePagingRequest();

                var applications = _getApplicationsQuery.Invoke(new GetApplicationsRequest
                    {
                        OrganisationId = organisationId,
                        Paging = pagingRequest
                    }).Applications;

                if (applications.Items == null || applications.Items.Count == 0)
                {
                    return View(new ApplicationsViewModel());
                }

                var applicationsViewModel = new ApplicationsViewModel
                    {
                        Applications = applications.Items.Select(Mapper.Map<Application, ApplicationViewModel>).ToList(),
                        Paging =
                            _pagingViewModelGenerator.Generate(PagingConstants.DefaultPagingId,
                                                               applications.PagingStatus, pagingRequest),
                        SystemView = true
                    };

                var users = _getUsersQuery.Invoke(new GetUsersRequest
                    {
                        OrganisationId = organisationId,
                        Paging = new PageRequestWithSort(1, Core.Configuration.MaxPageSize)
                    }).Users;

                foreach (var application in applicationsViewModel.Applications)
                {
                    var user = users.Items.FirstOrDefault(u => u.Id == application.DefaultUserId);
                    application.RuleMatchFactory = _matchRuleFactoryFactory.Create(application.RuleMatchFactory).Name;
                    application.DefaultUser = user == null
                                                  ? "Unknown"
                                                  : "{0} {1}".FormatWith(user.FirstName, user.LastName);
                }
                return View(applicationsViewModel);
            }
        }
    }
}
