
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Errordite.Web.Models.Groups
{
    public class AddGroupViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "GroupName_Required")]
        public string Name { get; set; }
        public IEnumerable<GroupMemberViewModel> Users { get; set; }
    }
}