using System.Collections.Generic;
using Errordite.Core.Paging;
using Errordite.Web.Models.Issues;

namespace Errordite.Web.Models.Dashboard
{
    public class ActivityViewModelBase : DashboardViewModelBase
    {
        public IEnumerable<IssueHistoryItemViewModel> Items { get; set; }
		public PagingStatus Paging { get; set; }
    }
}