using System.ComponentModel.DataAnnotations;
using Errordite.Web.Validation;

namespace Errordite.Web.Models.Authentication
{
    public class ResetPasswordViewModel
    {
        [RegularExpression(ValidationResources.Regexes.EmailAddress, ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Invalid")]
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Required")]
        public string Email { get; set; }
    }
}