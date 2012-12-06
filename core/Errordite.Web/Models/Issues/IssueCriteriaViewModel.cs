using System.Collections.Generic;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;

namespace Errordite.Web.Models.Issues
{
    public class BatchIssueActionForm
    {
        public List<string> IssueIds { get; set; }
        public IssueStatus Status { get; set; }
        public string Comment { get; set; }
        public string AssignToUser { get; set; }
        public BatchIssueAction Action { get; set; }
    }

    public enum BatchIssueAction
    {
        StatusUpdate,
        Delete
    }

    public class IssueCriteriaViewModel : IssueCriteriaPostModel
    {
        public IEnumerable<IssueItemViewModel> Issues { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Applications { get; set; }
        public PagingViewModel Paging { get; set; }
        public string ApplicationName { get; set; }
    }

    public class IssueCriteriaPostModel
    {
        public string[] Status { get; set; }
        public string ApplicationId { get; set; }
        public string AssignedTo { get; set; }
        public string Name { get; set; }
    }
}