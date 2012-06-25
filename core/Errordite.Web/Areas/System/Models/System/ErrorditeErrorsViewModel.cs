using System;
using CodeTrip.Core.Paging;
using Errordite.Core.Auditing;

namespace Errordite.Web.Models.Administration
{
    public class ErrorditeErrorsViewModel : ErrorditeErrorsPostModel
    {
        public Page<ErrorditeError> Errors { get; set; }
    }

    public class ErrorditeErrorsPostModel
    {
        public string Query { get; set; }
        public string ExceptionType { get; set; }
        public string Application { get; set; }
        public string MessageId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PagingViewModel Paging { get; set; }
    }
}