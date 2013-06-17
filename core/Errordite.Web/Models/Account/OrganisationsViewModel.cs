using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
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

    public class OrganisationSettingsViewModel : OrganisationSettingsPostModel
	{
        public IEnumerable<SelectListItem> Users { get; set; }
        public string ApiKey { get; set; }
	}

    public class OrganisationSettingsPostModel
    {
        public string TimezoneId { get; set; }
        public string PrimaryUserId { get; set; }
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

	public class ReplayReplacementsViewModel : ReplayReplacementsPostModel
	{
		public List<ReplayReplacement> ReplayReplacements { get; set; }
	}

	public class ReplayReplacementsPostModel
	{
		public Guid Id { get; set; }
		[Required(ErrorMessage = "Please enter the field this replacement should apply to")]
		public string Field { get; set; }
		[Required(ErrorMessage = "Please enter the string you want to be replaced")]
		public string Find { get; set; }
		[Required(ErrorMessage = "Please enter string you want to replace the Find string with")]
		public string Replace { get; set; }
	}
}