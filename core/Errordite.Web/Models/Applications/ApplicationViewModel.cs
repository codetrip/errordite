using System.Collections.Generic;
using CodeTrip.Core.Paging;

namespace Errordite.Web.Models.Applications
{
    public class ApplicationsViewModel
    {
        public bool SystemView { get; set; }
        public List<ApplicationViewModel> Applications { get; set; }
        public PagingViewModel Paging { get; set; }
    }

    public class ApplicationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RuleMatchFactory { get; set; }
        public string Token { get; set; }
        public string DefaultUser { get; set; }
        public string DefaultUserId { get; set; }
        public bool IsActive { get; set; }
        public int HipChatRoomId { get; set; }
        public string HipChatAuthToken { get; set; }
    }
}