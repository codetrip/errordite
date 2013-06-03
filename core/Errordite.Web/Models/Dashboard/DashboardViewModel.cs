using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;

namespace Errordite.Web.Models.Dashboard
{
    public class DashboardViewModel : DashboardLinksViewModel
    {
        public bool HasApplications { get; set; }
        public IEnumerable<ErrorInstanceViewModel> RecentErrors { get; set; }
        public IEnumerable<IssueItemViewModel> RecentIssues { get; set; }
        public string SingleApplicationId { get; set; }
        public string SingleApplicationToken { get; set; }
        public string TestIssueId { get; set; }
        public int LastIssueDisplayed { get; set; }
        public int LastErrorDisplayed { get; set; }
    }

    public class DashboardLinksViewModel
    {
        public Func<UrlHelper, string, string> UrlGetter { get; set; }
        public Statistics Stats { get; set; }
        public string SelectedApplicationName { get; set; }
        public string SelectedApplicationId { get; set; }
        public IEnumerable<Application> Applications { get; set; }
    }

    public enum DashboardTab
    {
        Home,
        Issues,
        Errors,
        Audit
    }
}