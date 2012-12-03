using System;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Error;

namespace Errordite.Web.Areas.System.Models.System
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