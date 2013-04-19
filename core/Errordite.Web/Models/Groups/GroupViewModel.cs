using System.Collections.Generic;
using Errordite.Core.Paging;

namespace Errordite.Web.Models.Groups
{
    public class GroupsViewModel
    {
        public List<GroupViewModel> Groups { get; set; }
        public PagingViewModel Paging { get; set; }
    }

    public class GroupViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public bool Disabled { get; set; }
    }
}