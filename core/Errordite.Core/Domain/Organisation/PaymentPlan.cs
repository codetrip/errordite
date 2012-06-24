

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
        public decimal Price { get; set; }
    }

    [ProtoContract]
    public enum PaymentPlanType
    {
        [ProtoMember(1)]
        Trial = 1,
        [ProtoMember(2)]
        Small = 2,
        [ProtoMember(3)]
        Medium = 3,
        [ProtoMember(4)]
        Large = 4
    }
}
