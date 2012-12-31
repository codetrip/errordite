using System;
using System.Web.Mvc;
using CodeTrip.Core.Extensions;
using Errordite.Core.Identity;
using Errordite.Core.Users.Queries;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;

namespace Errordite.Web.Areas.System.Controllers
{
    [Authorize, RoleAuthorize]
    public class ImpersonationController : AdminControllerBase
    {
        private readonly IImpersonationManager _impersonationManager;
        private readonly IGetUserQuery _getUserQuery;

        public ImpersonationController(IImpersonationManager impersonationManager, IGetUserQuery getUserQuery)
        {
            _impersonationManager = impersonationManager;
            _getUserQuery = getUserQuery;
        }

        [HttpGet, ImportViewData, GenerateBreadcrumbs(BreadcrumbId.AdminImpersonation)]
        public ActionResult Index(string userId = null, string organisationId = null)
        {
            var currentStatus = _impersonationManager.CurrentStatus;

            if (currentStatus.UserId.IsNullOrEmpty())
                currentStatus.UserId = userId;
            if (currentStatus.OrganisationId.IsNullOrEmpty())
                currentStatus.OrganisationId = organisationId;

            return View(currentStatus);
        }

        [HttpPost, ExportViewData]
        public ActionResult Set(ImpersonationStatus status)
        {
            if (status.Impersonating)
            {
                using (SwitchOrgScope(status.OrganisationId))
                {
                    var user = _getUserQuery.Invoke(new GetUserRequest
                        {
                            UserId = status.UserId,
                            OrganisationId = status.OrganisationId
                        }).User;

                    if (user == null)
                        return RedirectWithViewModel(status, "index",
                                                     "Failed to find user with Id {0}".FormatWith(status.UserId));

                    status.ExpiryUtc = DateTime.Now.AddMinutes(30);
                    status.EmailAddress = user.Email;
                    _impersonationManager.Impersonate(status);
                }
            }
            else
            {
                _impersonationManager.StopImpersonating();
            }

            ConfirmationNotification("Success!");
            return RedirectToAction("Index");
        }
    }
}