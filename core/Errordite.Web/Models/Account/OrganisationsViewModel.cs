using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Paging;
using Errordite.Web.Models.Users;

namespace Errordite.Web.Models.Account
{
    public class OrganisationsViewModel
    {
        public IEnumerable<OrganisationViewModel> Organisations { get; set; }
        public PagingViewModel Paging { get; set; }
    }

    public class OrganisationViewModel
    {
        public Organisation Organisation { get; set; }
        public Statistics Stats { get; set; }
    }

    public class OrganisationUsersViewModel : UsersViewModel
    { }

	public class OrganisationSettingsViewModel
	{
		public string TimezoneId { get; set; }
		public string ApiKey { get; set; }
		public string HipChatAuthToken { get; set; }
		public string CampfireToken { get; set; }
		public string CampfireCompany { get; set; }
		[Required(ErrorMessage = "Please enter a name for your organisation")]
		public string OrganisationName { get; set; }
	}

	public class CampfireSettingsViewModel
	{
		[Required(ErrorMessage = "Please enter your Campfire company")]
		public string CampfireCompany { get; set; }
		[Required(ErrorMessage = "Please enter your Campfire API authentication token")]
		public string CampfireToken { get; set; }
	}

	public class HipChatSettingsViewModel
	{
		[Required(ErrorMessage = "Please enter your HipChat authentication token")]
		public string HipChatAuthToken { get; set; }
	}
}