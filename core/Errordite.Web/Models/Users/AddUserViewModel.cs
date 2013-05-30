using System.Collections.Generic;
using Errordite.Web.Models.Groups;

namespace Errordite.Web.Models.Users
{
    public class AddUserViewModel : UserViewModelBase
    {
        public bool IsAdministrator { get; set; }
        public IList<GroupViewModel> Groups { get; set; }
    }
}