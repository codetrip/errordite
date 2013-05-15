using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Errordite.Web.Models.Groups;

namespace Errordite.Web.Models.Applications
{
    public class EditApplicationViewModel : EditApplicationPostModel
    {
        public IEnumerable<SelectListItem> ErrorConfigurations { get; set; }
		public IEnumerable<SelectListItem> Users { get; set; }
		public bool CampfireEnabled { get; set; }
		public bool HipChatEnabled { get; set; }
    }

    public class EditApplicationPostModel
    {
        public string Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resources.Application), ErrorMessageResourceName = "Name_Required")]
        public string Name { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public string MatchRuleFactoryId { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public int? HipChatRoomId { get; set; }
		public int? CampfireRoomId { get; set; }
        public List<GroupViewModel> NotificationGroups { get; set; }
    }
}