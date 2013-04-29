
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Errordite.Web.Models.Subscription
{
    public class SignUpViewModel : SignUpPostModel
    {
        public PaymentPlanViewModel SelectedPlan { get; set; }
        public IEnumerable<SelectListItem> CreditCards { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
    }

    public class SignUpPostModel
    {
		public string PaymentPlanId { get; set; }
		[Required(ErrorMessage = "Please select your credit card type")]
        public string CreditCard { get; set; }
        [Required(ErrorMessage = "Please enter your credit card number")]
        public string CreditCardNumber { get; set; }
        [Required(ErrorMessage = "Please enter the security code on teh back of your card")]
        public string SecurityCode { get; set; }
        [Required(ErrorMessage = "Please enter your name as it appears on your card.")]
        public string NameOnCard { get; set; }
        [Required(ErrorMessage = "Please enter the first line of your billing address.")]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        [Required(ErrorMessage = "Please enter the city or region for your billing address.")]
        public string CityRegion { get; set; }
        [Required(ErrorMessage = "Please enter the zip / postal code for your billing address.")]
        public string ZipPostalCode { get; set; }
        [Required(ErrorMessage = "Please select the country where your billing address is located.")]
        public string Country { get; set; }
    }
}