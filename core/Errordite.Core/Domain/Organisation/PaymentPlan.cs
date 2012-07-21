

using ProtoBuf;

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
        public int MaximumUsers { get; set; }
        [ProtoMember(4)]
        public int MaximumApplications { get; set; }
        [ProtoMember(5)]
        public int MaximumIssues { get; set; }
        [ProtoMember(6)]
        public decimal Price { get; set; }
        [ProtoMember(7)]
        public int Rank { get; set; }
        [ProtoMember(8)]
        public bool IsAvailable { get; set; }
        [ProtoMember(9)]
        public bool IsTrial { get; set; }
    }

    public static class PaymentPlanNames
    {
        public const string Trial = "Trial";
        public const string Micro = "Micro";
        public const string Small = "Small";
        public const string Big = "Big";
        public const string Huge = "Huge";
    }
}
