using System.Collections.Generic;
using Errordite.Core.Paging;
using Errordite.Web.Models.Issues;

namespace Errordite.Web.Models.Dashboard
{
    public class ActivityFeedViewModel : DashboardLinksViewModel
    {
        public IEnumerable<IssueHistoryItemViewModel> Feed { get; set; }
        public PagingViewModel Paging { get; set; }
    }
}