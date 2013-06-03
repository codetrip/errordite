
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Errordite.Web.Models.Groups
{
    public class EditGroupViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "GroupName_Required")]
        public string Name { get; set; }
        public IEnumerable<GroupMemberViewModel> Users { get; set; }
    }

    public class GroupMemberViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool Selected { get; set; }
    }
}