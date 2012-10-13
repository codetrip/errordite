
using System.ComponentModel.DataAnnotations;
using Errordite.Web.Validation;

namespace Errordite.Web.Models.Beta
{
    public class SignUpViewModel
    {
        public string Id { get; set; }
        [RegularExpression(ValidationResources.Regexes.EmailAddress, ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Invalid")]
        [Required(ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Required")]
        public string EmailAddress { get; set; }
    }
}