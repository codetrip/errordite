

using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public class PaymentPlan
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public PaymentPlanType PlanType { get; set; }
        [ProtoMember(3)]
        public int MaximumUsers { get; set; }
        [ProtoMember(4)]
        public int MaximumApplications { get; set; }
        [ProtoMember(5)]
        public int MaximumIssues { get; set; }
        [ProtoMember(6)]
        public decimal Price { get; set; }
    }

    [ProtoContract]
    public enum PaymentPlanType
    {
        [ProtoMember(1)]
        Trial = 1,
        [ProtoMember(2)]
        Micro = 2,
        [ProtoMember(3)]
        Small = 3,
        [ProtoMember(4)]
        Big = 4,
        [ProtoMember(5)]
        Huge = 5
    }
}
