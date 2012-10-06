using System.Collections.Generic;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;

namespace Errordite.Web.Models.Dashboard
{
    public class DashboardViewModel
    {
        public bool HasApplications { get; set; }
        public Statistics Stats { get; set; }
        public IEnumerable<ErrorInstanceViewModel> RecentErrors { get; set; }
        public IEnumerable<IssueItemViewModel> RecentIssues { get; set; }
        public string SingleApplicationId { get; set; }
        public string TestIssueId { get; set; }
    }

    public enum DashboardTab
    {
        Home,
        Issues,
        Errors,
        Audit
    }
}