using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;
using App = Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Models.Errors
{
    public class ErrorPageViewModel
    {
        public ErrorCriteriaViewModel ErrorsViewModel { get; set; }
        public string ApplicationName { get; set; }
    }

    public class ErrorCriteriaViewModel : ErrorCriteriaPostModel
    {
        public bool HideIssues { get; set; }
        public IList<ErrorInstanceViewModel> Errors { get; set; }
        public IEnumerable<SelectListItem> Applications { get; set; }
    }

    public class ErrorInstanceViewModel
    {
        public Error Error { get; set; }
        public bool HideIssues { get; set; }
    }

    [Serializable]
    public class ErrorCriteriaPostModel
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string IssueId { get; set; }
        public string Query { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ApplicationId { get; set; }
        public PagingViewModel Paging { get; set; }
    }
}