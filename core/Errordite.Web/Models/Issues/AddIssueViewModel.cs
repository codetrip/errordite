
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Errordite.Core.Domain.Error;

namespace Errordite.Web.Models.Issues
{
    public class AddIssueViewModel : AddIssuePostModel
    {
        public IEnumerable<SelectListItem> Users { get; set; }
        public IEnumerable<SelectListItem> Applications { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
    }

    public class AddIssuePostModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.IssueResources), ErrorMessageResourceName = "Name_Required")]
        public string Name { get; set; }
        public string UserId { get; set; }
        public string ApplicationId { get; set; }
        public IssueStatus Status { get; set; }
        public IList<RuleViewModel> Rules { get; set; }
    }
}