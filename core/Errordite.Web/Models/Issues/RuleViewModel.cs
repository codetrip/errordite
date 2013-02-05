using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Errordite.Core.Matching;

namespace Errordite.Web.Models.Issues
{
    public class RuleViewModel
    {
        public int Index { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resources.Rules), ErrorMessageResourceName = "ErrorProperty_Required")]
        public string ErrorProperty { get; set; }
        public StringOperator StringOperator { get; set; }
        public string Value { get; set; }
        public IEnumerable<SelectListItem> Properties { get; set; }
    }

    public class IssueRulesPostModel
    {
        public string Id { get; set; }
        public string ApplicationId { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resources.Rules), ErrorMessageResourceName = "IssueName_Required")]
        public string UnmatchedIssueName { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resources.Rules), ErrorMessageResourceName = "IssueName_Required")]
        public string IssueNameAfterUpdate { get; set; }
        public IList<RuleViewModel> Rules { get; set; }
    }
}