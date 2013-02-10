
using System;
using CodeTrip.Core.Extensions;
using Errordite.Core.Authorisation;
using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public class Organisation : IOrganisationEntity
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public OrganisationStatus Status { get; set; }
        [ProtoMember(4)]
        public string PaymentPlanId { get; set; }
        [ProtoMember(5)]
        public DateTime CreatedOnUtc { get; set; }
        [Raven.Imports.Newtonsoft.Json.JsonIgnore, ProtoMember(6)]
        public PaymentPlan PaymentPlan { get; set; }
        [ProtoMember(7)]
        public string TimezoneId { get; set; }
        [ProtoMember(8)]
        public SuspendedReason? SuspendedReason { get; set; }
        [ProtoMember(9)]
        public string SuspendedMessage { get; set; }
        [ProtoMember(10)]
        public string ApiKey { get; set; }
        [ProtoMember(11)]
        public string ApiKeySalt { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }
        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "organisations/{0}".FormatWith(friendlyId);
        }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string OrganisationId
        {
            get { return Id; }
        }

    }

    [ProtoContract]
    public enum OrganisationStatus
    {
        [ProtoMember(1)]
        Active,
        [ProtoMember(2)]
        Suspended,
        [ProtoMember(3)]
        PlanQuotaExceeded
    }

    [ProtoContract]
    public enum SuspendedReason
    {
        [ProtoMember(1)]
        RequestedByAccountHolder,
        [ProtoMember(2)]
        PaymentArrears,
        [ProtoMember(3)]
        Other
    }
}
