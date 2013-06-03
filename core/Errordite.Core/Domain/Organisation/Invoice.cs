using System;
using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
	[ProtoContract]
	public class Invoice
	{
		[ProtoMember(1)]
		public string Id { get; set; }
		[ProtoMember(2)]
		public decimal Amount { get; set; }
		[ProtoMember(3)]
		public DateTimeOffset ProcessedOn { get; set; }
		[ProtoMember(4)]
		public string PaymentPlanId { get; set; }
		[ProtoMember(5)]
		public string OrganisationId { get; set; }
	}
}
