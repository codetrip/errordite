using System.ComponentModel.DataAnnotations;
using Errordite.Web.Validation;

namespace Errordite.Web.Models.Users
{
    public class UserWithCredentialsViewModelBase : UserViewModelBase
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Password_Required")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "ConfirmPassword_Required")]
        [Match(PropertyName = "Password", ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Password_Mismatch")]
        public string ConfirmPassword { get; set; }
    }

    public class UserViewModelBase
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "FirstName_Required")]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "LastName_Required")]
        public string LastName { get; set; }

        [RegularExpression(ValidationResources.Regexes.EmailAddress, ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Invalid")]
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Required")]
        public string Email { get; set; }

        public string TimezoneId { get; set; }
    }
}