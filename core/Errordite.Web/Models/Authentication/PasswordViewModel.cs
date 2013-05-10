using System.ComponentModel.DataAnnotations;
using Errordite.Web.Validation;

namespace Errordite.Web.Models.Authentication
{
    public class PasswordViewModel
    {
        public string Token { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Password_Required")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "ConfirmPassword_Required")]
        [Match(PropertyName = "Password", ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Password_Mismatch")]
        public string ConfirmPassword { get; set; }
    }
}