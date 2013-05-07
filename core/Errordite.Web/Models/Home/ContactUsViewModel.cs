
using System.ComponentModel.DataAnnotations;
using Errordite.Web.Validation;

namespace Errordite.Web.Models.Home
{
    public class ContactUsViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "ContactUsViewModel_Name", ErrorMessageResourceType = typeof(Resources.Home))]
        public string Name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "ContactUsViewModel_Email", ErrorMessageResourceType = typeof(Resources.Home))]
        [RegularExpression(ValidationResources.Regexes.EmailAddress, ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "Email_Invalid")]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "ContactUsViewModel_Message", ErrorMessageResourceType = typeof(Resources.Home))]
        public string Message { get; set; }
        public ContactUsReason Reason { get; set; }
    }

    public enum ContactUsReason
    {
        GeneralQuestion,
        ReportIssue,
        PricingInfo
    }
}