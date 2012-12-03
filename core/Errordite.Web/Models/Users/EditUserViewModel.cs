
using System.Collections.Generic;
using Errordite.Web.Models.Groups;

namespace Errordite.Web.Models.Users
{
    public class EditUserViewModel : UserViewModelBase
    {
        public IList<GroupViewModel> Groups { get; set; }
        public string UserId { get; set; }
        public bool CurrentUser { get; set; }
        public bool IsAdministrator { get; set; }
    }
}