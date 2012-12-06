
using System.ComponentModel.DataAnnotations;
using CodeTrip.Core.Extensions;

namespace Errordite.Web.Validation
{
    public class MatchAttribute : ValidationAttribute
    {
        public string PropertyName { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var valueToValidateWith = validationContext.ObjectInstance.PropertyValue(PropertyName);

            var isValid = (string)valueToValidateWith == (string)value;

            return isValid ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }
    }
}