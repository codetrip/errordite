using System;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Domain.Organisation
{
    public class AuditRecord
    {
        public string Id { get; set; }
        public string OrganisationId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string TriggeredBy { get; set; }
        public DateTime QueuedOnUtc { get; set; }
        public DateTime CompletedOnUtc { get; set; }
        public AuditRecordStatus Status { get; set; }
        public AuditRecordType AuditRecordType { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "auditrecords/{0}".FormatWith(friendlyId);
        }
    }

    public enum AuditRecordStatus
    {
        Success,
        Failed
    }

    public enum AuditRecordType
    {
        BackgroundTask,
        SystemTask
    }
}
