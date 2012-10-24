using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CodeTrip.Core.Paging;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Models.Audit
{
    public class AuditViewModel : AuditPostModel
    {
        public Page<AuditRecord> AuditRecords { get; set; }
        public IEnumerable<SelectListItem> AuditRecordTypes { get; set; }
        public IEnumerable<SelectListItem> AuditRecordStatuses { get; set; }
    }

    public class AuditPostModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AuditRecordStatus? Status { get; set; }
        public PagingViewModel Paging { get; set; }
        public AuditRecordType? AuditRecordType { get; set; }
    }
}