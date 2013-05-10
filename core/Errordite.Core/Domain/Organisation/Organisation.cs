using System;
using System.ComponentModel;
using Errordite.Core.Domain.Master;
using Errordite.Core.Extensions;
using Errordite.Core.Authorisation;
using ProtoBuf;
using Raven.Imports.Newtonsoft.Json;

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
        [JsonIgnore, ProtoMember(6)]
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
        [ProtoMember(12)]
		public string RavenInstanceId { get; set; }
        [JsonIgnore, ProtoMember(13)]
		public RavenInstance RavenInstance { get; set; }
		[ProtoMember(14)]
        public Subscription Subscription { get; set; }
        [ProtoMember(15)]
        public string PrimaryUserId { get; set; }
        [ProtoMember(16)]
		public int QuotasExceededReminders { get; set; }
		[ProtoMember(17)]
		public DateTime? SuspendedOnUtc { get; set; }
		[ProtoMember(18)]
		public string HipChatAuthToken { get; set; }
		[ProtoMember(19)]
		public CampfireDetails CampfireDetails { get; set; }

        [JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }
        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "organisations/{0}".FormatWith(friendlyId);
        }

        [JsonIgnore]
        public string OrganisationId
        {
            get { return Id; }
        }

        public static string NullOrganisationId
        {
            get { return "null"; }
        }
	}

	public class CampfireDetails
	{
		[ProtoMember(1)]
		public string Token { get; set; }
		[ProtoMember(2)]
		public string Company { get; set; }
	}

	public class Subscription
	{
		[ProtoMember(1)]
		public int? ChargifyId { get; set; }
		[ProtoMember(2)]
		public SubscriptionStatus Status { get; set; }
		[ProtoMember(3)]
		public DateTimeOffset StartDate { get; set; }
		[ProtoMember(4)]
		public DateTimeOffset CurrentPeriodEndDate { get; set; }
		[ProtoMember(5)]
		public DateTimeOffset LastModified { get; set; }
		[ProtoMember(6)]
		public string CancellationReason { get; set; }
		[ProtoMember(7)]
		public DateTimeOffset? CancellationDate { get; set; }
	}

    [ProtoContract]
    public enum OrganisationStatus
    {
        [ProtoMember(1)]
        Active,
        [ProtoMember(2)]
        Suspended,
        [ProtoMember(3)]
        PlanQuotaExceeded,
    }

	[ProtoContract]
	public enum SubscriptionStatus
	{
		[ProtoMember(1)]
		Trial,
		[ProtoMember(2)]
		Active,
		[ProtoMember(3)]
		Query,
		[ProtoMember(4)]
		Cancelled
	}

    [ProtoContract]
    public enum SuspendedReason
    {
        [ProtoMember(1), Description("you cancelled your subscription")]
        SubscriptionCancelled,
        [ProtoMember(2), Description("we have not been able to take payment for your subscription")]
        PaymentArrears,
        [ProtoMember(3), Description("please contact support for further information")]
        Other
    }
}
