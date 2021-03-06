﻿

using Errordite.Core.Extensions;
using ProtoBuf;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public class PaymentPlan
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public int MaximumIssues { get; set; }
        [ProtoMember(4)]
        public decimal Price { get; set; }
        [ProtoMember(5)]
        public int Rank { get; set; }
		[ProtoMember(6)]
		public string SignUpUrl { get; set; }
		[JsonIgnore]
	    public bool IsFreeTier
	    {
			get { return Price == 0; }
	    }

		[JsonIgnore]
		public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "PaymentPlans/{0}".FormatWith(friendlyId);
        }

        public PaymentPlanType Type { get; set; }

        public string SpecialId { get; set; }
    }

    public enum PaymentPlanType
    {
        Standard,
        AppHarbor,
        Custom
    }

    public static class PaymentPlanNames
    {
        public const string Free = "Free";
        public const string Small = "Small";
        public const string Medium = "Medium";
        public const string Large = "Large";
    }
}
