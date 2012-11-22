using System.ComponentModel.DataAnnotations;
using Errordite.Web.Models.Users;

namespace Errordite.Web.Models.Authentication
{
    public class RegisterViewModel : UserWithCredentialsViewModelBase
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Organisation_Required")]
        public string OrganisationName { get; set; }
    }
}