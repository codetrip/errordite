﻿
using System;
using System.Web.Mvc;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using CodeTrip.Core.Extensions;
using Errordite.Core.Identity;
using Errordite.Web.Models.Issues;
using System.Linq;

namespace Errordite.Web.Extensions
{
    public static class UrlExtensions
    {
        #region Dashboard

        public static string Audit(this UrlHelper helper)
        {
            return helper.Action("index", "audit", new { Area = string.Empty });
        }

        public static string Dashboard(this UrlHelper helper)
        {
            return helper.Action("index", "dashboard", new { Area = string.Empty });
        }
        
        public static string Errors(this UrlHelper helper, string applicationId = null)
        {
            return applicationId != null ? helper.Action("index", "errors", new { applicationId, Area = string.Empty }) : helper.Action("index", "errors", new { Area = string.Empty });
        }

        public static string ClearErrors(this UrlHelper helper, string applicationId, string issueId)
        {
            if(issueId.IsNullOrEmpty())
            {
                return applicationId.IsNotNullOrEmpty() ? helper.Action("index", "errors", new { applicationId, Area = string.Empty }) : helper.Action("index", "errors", new { Area = string.Empty });
            }

            return helper.Action("index", "issue", new { Id = issueId.GetFriendlyId(), Tab = IssueTab.Errors.ToString(), Area = string.Empty });
        }

        public static string AllIssues(this UrlHelper helper)
        {
            string root = helper.Action("index", "issues", new { Area = string.Empty });
            string status = Enum.GetNames(typeof(IssueStatus)).Aggregate("?", (current, t) => current + ("Status={0}&".FormatWith(t)));
            return root + status;
        }

        public static string Issues(this UrlHelper helper, string applicationId = null)
        {
            return applicationId != null ? helper.Action("index", "issues", new { applicationId, Area = string.Empty }) : helper.Action("index", "issues", new { Area = string.Empty });
        }

        public static string AddIssue(this UrlHelper helper)
        {
            return helper.Action("add", "issues", new { Area = string.Empty });
        }

        public static string Issue(this UrlHelper helper, string id, IssueTab? tab = null)
        {
            id = IdHelper.GetFriendlyId(id);
            return helper.Action("index", "issue", tab != null ? (object)new { id, tab = tab.ToString().ToLowerInvariant(), Area = string.Empty } : new { id, Area = string.Empty });
        }

        public static string BaseIssueUrl(this UrlHelper helper)
        {
            return helper.Action("index", "issue", new {id = "{0}", Area = string.Empty});
        }

        public static string Issues(this UrlHelper helper, IssueStatus status)
        {
            return helper.Action("index", "issues", new { Status = status, Area = string.Empty });
        }

        public static string MyIssues(this UrlHelper helper, string userId)
        {
            return helper.Action("index", "issues", new { AssignedTo = userId, Area = string.Empty });
        }

        public static string Rules(this UrlHelper helper, string id)
        {
            return helper.Action("adjustrules", "issue", new { id, Area = string.Empty });
        }

        #endregion

        #region Home

        public static string Home(this UrlHelper helper, AppContext context = null)
        {
            return context == null || context.AuthenticationStatus == AuthenticationStatus.Anonymous ? "/" : helper.Dashboard();
        }

        public static string Contact(this UrlHelper helper)
        {
            return "/contact";
        }

        #endregion

        #region Help

        public static string ClientDownload(this UrlHelper helper)
        {
            return "http://errordite.codeplex.com/releases";
        }

        public static string ClientSource(this UrlHelper helper)
        {
            return "http://errordite.codeplex.com/SourceControl/list/changesets";
        }
        
        public static string ClientCodeplex(this UrlHelper helper)
        {
            return "http://errordite.codeplex.com";
        }

        public static string Client(this UrlHelper helper)
        {
            return helper.Action("client", "help", new { Area = string.Empty });
        }

        public static string Features(this UrlHelper helper)
        {
            return helper.Action("features", "help", new { Area = string.Empty });
        }

        public static string Faq(this UrlHelper helper)
        {
            return helper.Action("faq", "help", new { Area = string.Empty });
        }

        public static string WhatIsErrordite(this UrlHelper helper)
        {
            return helper.Action("whatisit", "help", new {Area = ""});
        }

        public static string GettingStarted(this UrlHelper helper)
        {
            return helper.Action("gettingstarted", "help", new { Area = string.Empty });
        }

        public static string Pricing(this UrlHelper helper)
        {
            return helper.Action("pricing", "help", new { Area = string.Empty });
        }

        public static string Privacy(this UrlHelper helper)
        {
            return helper.Action("privacy", "help", new { Area = string.Empty });
        }

        public static string TermsAndConditions(this UrlHelper helper)
        {
            return helper.Action("termsandconditions", "help", new { Area = string.Empty });
        }

        #endregion

        #region Authentication

        public static string SignUp(this UrlHelper helper)
        {
            return helper.Action("signup", "authentication", new { Area = string.Empty });
        }

        public static string SignIn(this UrlHelper helper)
        {
            return helper.Action("signin", "authentication", new { Area = string.Empty });
        }

        public static string SignOut(this UrlHelper helper)
        {
            return helper.Action("signout", "authentication", new { Area = string.Empty });
        }

        public static string ResetPassword(this UrlHelper helper)
        {
            return helper.Action("resetpassword", "authentication", new { Area = string.Empty });
        }

        #endregion

        #region Users

        public static string Users(this UrlHelper helper, string groupId = null)
        {
            return groupId != null ? helper.Action("index", "users", new { groupId, Area = string.Empty }) : helper.Action("index", "users", new { Area = string.Empty });
        }

        public static string AddUser(this UrlHelper helper)
        {
            return helper.Action("add", "users", new { Area = string.Empty });
        }

        public static string YourDetails(this UrlHelper helper)
        {
            return helper.Action("yourdetails", "users", new { Area = string.Empty });
        }

        public static string EditUser(this UrlHelper helper, string userId)
        {
            return helper.Action("edit", "users", new { userId, Area = string.Empty });
        }

        #endregion
        
        #region Groups

        public static string Groups(this UrlHelper helper)
        {
            return helper.Action("index", "groups", new { Area = string.Empty });
        }

        public static string AddGroup(this UrlHelper helper)
        {
            return helper.Action("add", "groups", new { Area = string.Empty });
        }

        public static string EditGroup(this UrlHelper helper, string groupId)
        {
            return helper.Action("edit", "groups", new { groupId, Area = string.Empty });
        }

        #endregion

        #region Admin

        public static string PaymentPlan(this UrlHelper helper)
        {
            return helper.Action("paymentplan", "admin", new {Area = string.Empty});
        }

        public static string Billing(this UrlHelper helper)
        {
            return helper.Action("billing", "admin", new { Area = string.Empty });
        }

        public static string Settings(this UrlHelper helper)
        {
            return helper.Action("settings", "admin", new { Area = string.Empty });
        }

        public static string Upgrade(this UrlHelper helper)
        {
            return helper.Action("upgrade", "admin", new { Area = string.Empty });
        }

        public static string Downgrade(this UrlHelper helper)
        {
            return helper.Action("downgrade", "admin", new { Area = string.Empty });
        }

        #endregion

        #region Applications

        public static string Applications(this UrlHelper helper)
        {
            return helper.Action("index", "applications", new { Area = string.Empty });
        }

        public static string AddApplication(this UrlHelper helper, bool? newOrganisation = null)
        {
            return helper.Action("add", "applications", new { Area = string.Empty, newOrganisation });
        }

        public static string EditApplication(this UrlHelper helper, string applicationId)
        {
            return helper.Action("edit", "applications", new { applicationId, Area = string.Empty });
        }

        #endregion

        #region Administration

        public static string Cache(this UrlHelper helper, string cacheProfile)
        {
            return helper.Action("index", "cache", new { id = cacheProfile, Area = WebConstants.AreaNames.System });
        }

        public static string FlushAllCaches(this UrlHelper helper)
        {
            return helper.Action("flushallcaches", "cache", new { Area = WebConstants.AreaNames.System });
        }

        public static string SysAdmin(this UrlHelper helper)
        {
            return helper.Action("index", "system", new { Area = WebConstants.AreaNames.System });
        }

        public static string ErrorditeErrors(this UrlHelper helper)
        {
            return helper.Action("errorditeerrors", "system", new { Area = WebConstants.AreaNames.System });
        }

        public static string Organisations(this UrlHelper helper)
        {
            return helper.Action("index", "organisations", new {Area = WebConstants.AreaNames.System});
        }

        public static string Impersonate(this UrlHelper helper, string userId = null, string organisationId = null)
        {
            return helper.Action("index", "impersonation", new { userId, organisationId, Area = WebConstants.AreaNames.System });
        }

        public static string OrganisationUsers(this UrlHelper helper, string organisationId)
        {
            return helper.Action("users", "organisations", new { organisationId, Area = WebConstants.AreaNames.System});
        }

        public static string OrganisationApplications(this UrlHelper helper, string organisationId)
        {
            return helper.Action("applications", "organisations", new { organisationId, Area = WebConstants.AreaNames.System });
        }

        #endregion
    }
}