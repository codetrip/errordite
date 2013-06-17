using System.Collections.Generic;
using System.Web.Mvc;

namespace Errordite.Web.Models.Test
{
	public class GenerateErrorViewModel : GenerateErrorPostModel
	{
		public IEnumerable<SelectListItem> Errors { get; set; }
		public IEnumerable<SelectListItem> Applications { get; set; }

		public static List<SelectListItem> GetErrors(string selected)
		{
			return new List<SelectListItem>
			{
				new SelectListItem {Text = "Argument Null Exception", Value = "1", Selected = selected == "1"},
				new SelectListItem {Text = "Null Reference Exception", Value = "2", Selected = selected == "2"},
				new SelectListItem {Text = "Divide By Zero Exception", Value = "3", Selected = selected == "3"},
				new SelectListItem {Text = "Invalid Operation Exception", Value = "4", Selected = selected == "4"},
				new SelectListItem {Text = "Divide By Zero Exception", Value = "5", Selected = selected == "5"},
			};
		}
	}

	public class GenerateErrorPostModel
	{
		public string ErrorId { get; set; }
		public string Token { get; set; }
		public string Json { get; set; }
	}
}