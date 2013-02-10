using System.Collections.Generic;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues.Queries;
using Errordite.Web.Models.Errors;
using Errordite.Web.Models.Issues;

namespace Errordite.Web.Models.Search
{
    public class SearchViewModel
    {
		public string Query { get; set; }
		public int IssueTotal { get; set; }
		public int ErrorTotal { get; set; }
	    public IEnumerable<ErrorInstanceViewModel> Errors { get; set; }
        public IEnumerable<IssueItemViewModel> Issues { get; set; }

        public Issue IssueWithMatchingId { get; set; }
    }
}