using System.Collections.Generic;

namespace Errordite.Web.Models.Common
{
    public interface IWizardViewModel
    {
        Dictionary<string, List<string>> IssuesPerPage { get; set; }
        string IssueIds { get; set; }
        bool? Next { get; set; }
        int PageNumber { get; set; }
    }
}