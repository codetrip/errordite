using System.ComponentModel.DataAnnotations;
using Errordite.Web.Validation;

namespace Errordite.Web.Models.Authentication
{
    public class LoginViewModel
    {
        [RegularExpression(ValidationResources.Regexes.EmailAddress, ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Invalid")]
        [Required(ErrorMessageResourceType = typeof(Resources.Authentication), ErrorMessageResourceName = "Email_Required")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Authentication), ErrorMessageResourceName = "Password_Required")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}