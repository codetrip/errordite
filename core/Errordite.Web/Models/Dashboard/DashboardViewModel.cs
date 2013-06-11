using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;

namespace Errordite.Web.Models.Dashboard
{
	public class DashboardViewModel : DashboardViewModelBase
	{
		public static List<DashboardSort> Sorting = new List<DashboardSort>
		{
			new DashboardSort("1", "Issues By Most Recent Error", true, "LastErrorUtc"),
			new DashboardSort("2", "Issues By Most Recently Created", true, "CreatedOnUtc"),
			new DashboardSort("3", "Issues By Highest Error Count", true, "ErrorCount"),
			new DashboardSort("4", "Issues By Lowest Error Count", false, "ErrorCount"),
			new DashboardSort("5", "Errors By Most Recently Created", false, string.Empty),
		};

		public bool HasApplications { get; set; }
		public IEnumerable<IssueItemViewModel> Issues { get; set; }
		public IEnumerable<ErrorInstanceViewModel> Errors { get; set; }
        public string SingleApplicationId { get; set; }
        public string SingleApplicationToken { get; set; }
		public string TestIssueId { get; set; }
		public bool ShowIntro { get; set; }
		public string ShowMe { get; set; }
		public int PageSize { get; set; }
		public IEnumerable<SelectListItem> ShowMeOptions { get; set; }
		public IEnumerable<SelectListItem> PageSizes { get; set; }
    }

    public class DashboardViewModelBase
    {
        public Func<UrlHelper, string, string> UrlGetter { get; set; }
        public string SelectedApplicationName { get; set; }
        public string SelectedApplicationId { get; set; }
        public IEnumerable<Application> Applications { get; set; }
    }

	public class DashboardSort
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public bool SortDescending { get; set; }
		public string SortField { get; set; }

		public DashboardSort(string id, string displayName, bool sortDescending, string sortField)
		{
			Id = id;
			DisplayName = displayName;
			SortDescending = sortDescending;
			SortField = sortField;
		}
	}

    public enum NavTabs
    {
        Dashboard,
        Issues,
        Errors,
        Activity,
		Account,
		Docs,
		AddIssue,
		None,
		Contact
    }
}