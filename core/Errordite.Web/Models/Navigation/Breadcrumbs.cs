using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Web.Extensions;
using Errordite.Core.Extensions;

namespace Errordite.Web.Models.Navigation
{
    public class Breadcrumbs
    {
        private static IEnumerable<Breadcrumb> _breadcrumbs;
        private static readonly object _syncLock = new object();
        
        public static Breadcrumb GetById(BreadcrumbId id, UrlHelper urlHelper)
        {
            CreateBreadcrumbsIfRequired(urlHelper);

            var item = _breadcrumbs.FirstOrDefaultFromMany(n => n.Children, n => n.Id == id);

            return item;
        }

        public static List<Breadcrumb> GetBreadcrumbsForRoute(BreadcrumbId id, UrlHelper urlHelper)
        {
            CreateBreadcrumbsIfRequired(urlHelper);

            var item = _breadcrumbs.FirstOrDefaultFromMany(n => n.Children, n => n.Id == id);

            if(item != null)
                return GetPath(item);

            return null;
        }

        private static void CreateBreadcrumbsIfRequired(UrlHelper urlHelper)
        {
            if (_breadcrumbs == null)
            {
                lock (_syncLock)
                {
                    if (_breadcrumbs == null)
                    {
                        CreateBreadcrumbs(urlHelper);
                    }
                }
            }
        }

        public static List<Breadcrumb> GetPath(Breadcrumb child)
        {
            var items = new List<Breadcrumb>();

            Breadcrumb current = child;

            while (current != null)
            {
                items.Add(current);
                current = current.Parent;
            }

            items.Reverse();
            return items;
        }

        private static void CreateBreadcrumbs(UrlHelper urlHelper)
        {
            _breadcrumbs = new []
            {
                new Breadcrumb(BreadcrumbId.Dashboard, urlHelper.Dashboard(), "Dashboard", new []
                {
                    new Breadcrumb(BreadcrumbId.Issues, urlHelper.Issues(), "Issues", new []
                    {
                        new Breadcrumb(BreadcrumbId.AddIssue, string.Empty, "Add Issue"),
                        new Breadcrumb(BreadcrumbId.Issue, string.Empty, "Issue"),
                    }),
                    new Breadcrumb(BreadcrumbId.Errors, string.Empty, "Errors")
                }),  
                new Breadcrumb(BreadcrumbId.Home, urlHelper.Home(), "Home", new []
                {
                    //new Breadcrumb(BreadcrumbId.WhatIsIt, string.Empty, "What Is It"),
                    new Breadcrumb(BreadcrumbId.About, "", "About"), 
                    new Breadcrumb(BreadcrumbId.QuickStart, string.Empty, "Quick Start"),
                    new Breadcrumb(BreadcrumbId.Clients, string.Empty, "Client"),
                    new Breadcrumb(BreadcrumbId.Pricing, string.Empty, "Pricing"),
                    new Breadcrumb(BreadcrumbId.Features, string.Empty, "Features"),
                    new Breadcrumb(BreadcrumbId.Privacy, string.Empty, "Privacy"),
                    new Breadcrumb(BreadcrumbId.TermsAndConditions, string.Empty, "Terms and Conditions")
                }),  
                new Breadcrumb(BreadcrumbId.Admin, null, "Admin", new []
                {
                    new Breadcrumb(BreadcrumbId.Subscription, urlHelper.Subscription(), "Subscription", new []
                    {
                        new Breadcrumb(BreadcrumbId.SubscriptionHome, string.Empty, "Subscription"),
                        new Breadcrumb(BreadcrumbId.BillingHistory, string.Empty, "Billing History"),
                        new Breadcrumb(BreadcrumbId.ChangeSubscription, string.Empty, "Change Subscription"),
                        new Breadcrumb(BreadcrumbId.CancelSubscription, string.Empty, "Cancel Subscription"),
                        new Breadcrumb(BreadcrumbId.SubscriptionSignUp, string.Empty, "Subscription Sign Up"),
                        new Breadcrumb(BreadcrumbId.SubscriptionSignUpFailed, string.Empty, "Subscription Sign Up Failed")
                    }),
                    new Breadcrumb(BreadcrumbId.Applications, urlHelper.Applications(), "Applications", new []
                    {
                        new Breadcrumb(BreadcrumbId.AddApplication, string.Empty, "Add Application"),
                        new Breadcrumb(BreadcrumbId.EditApplication, string.Empty, "Edit Application")
                    }),
                    new Breadcrumb(BreadcrumbId.Users, urlHelper.Users(), "Users", new []
                    {
                        new Breadcrumb(BreadcrumbId.AddUser, string.Empty, "Add User"),
                        new Breadcrumb(BreadcrumbId.EditUser, string.Empty, "Edit User"),
                        new Breadcrumb(BreadcrumbId.EditYourDetails, string.Empty, "Edit Your Details")
                    }),
                    new Breadcrumb(BreadcrumbId.Groups, urlHelper.Groups(), "Groups", new []
                    {
                        new Breadcrumb(BreadcrumbId.AddGroup, string.Empty, "Add Group"),
                        new Breadcrumb(BreadcrumbId.EditGroup, string.Empty, "Edit Group")
                    }),
                    new Breadcrumb(BreadcrumbId.Settings, urlHelper.Timezone(), "Settings"),
                }),
                new Breadcrumb(BreadcrumbId.SysAdmin, urlHelper.SystemAdmin(), "System Admin", new []
                {
                    new Breadcrumb(BreadcrumbId.AdminErrors, string.Empty, "Errors"),
                    new Breadcrumb(BreadcrumbId.AdminImpersonation, string.Empty, "Impersonation"),
                    new Breadcrumb(BreadcrumbId.AdminOrganisations, urlHelper.Organisations(), "Organisations", new []
                    {
                        new Breadcrumb(BreadcrumbId.AdminUsers, string.Empty, "Users"),
                        new Breadcrumb(BreadcrumbId.AdminApplications, string.Empty, "Applications")
                    }),
                    new Breadcrumb(BreadcrumbId.AdminFlushCaches, urlHelper.FlushAllCaches(), "Flush Caches", new []
                    {
                        new Breadcrumb(BreadcrumbId.AdminCache, string.Empty, "Cache")
                    }),
                }),  
            };
        }
    }

    public class Breadcrumb
    {
        public BreadcrumbId Id { get; private set; }
        public Breadcrumb Parent { get; private set; }
        public string Link { get; private set; }
        public string Title { get; private set; }
        public IEnumerable<Breadcrumb> Children { get; private set; }
        
        public Breadcrumb(BreadcrumbId id, string link, string title, IEnumerable<Breadcrumb> children = null)
        {
            Id = id;
            Link = link;
            Title = title;
            Children = children ?? new List<Breadcrumb>();

            foreach (var child in Children)
                child.Parent = this;
        }
    }

    public enum BreadcrumbId
    {
        Home,
        Dashboard,
        Issues,
        AddIssue,
        Errors,
        MergeIssues,
        Issue,

        QuickStart,
        Pricing,
        Clients,
        Features,
        TermsAndConditions,
        Privacy,

        Admin,
		Billing,
		BillingHistory,
        Subscription,
		SubscriptionHome,
		ChangeSubscription,
		CancelSubscription,
		SubscriptionSignUp,
		SubscriptionSignUpFailed,
        Settings,

        Applications,
        AddApplication,
        EditApplication,

        Users,
        AddUser,
        EditUser,
		EditYourDetails,

        Groups,
        AddGroup,
        EditGroup,

        SysAdmin,
        AdminErrors,
        AdminImpersonation,
        AdminUsers,
        AdminApplications,
        AdminOrganisations,
        AdminFlushCaches,
        AdminCache,
        About
    }
}
