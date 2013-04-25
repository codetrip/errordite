using System.Collections.Generic;
using Errordite.Core.Paging;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Models.Users
{
    public class UsersViewModel
    {
        public List<UserViewModel> Users { get; set; }
        public PagingViewModel Paging { get; set; }
        public bool EnableImpersonation { get; set; }
        public string OrganisationId { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Groups { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
    }
}