using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Errordite.Core.Domain.Master
{
	[ProtoContract]
    public class UserOrganisationMapping
    {
		[ProtoMember(1)]
		public string EmailAddress { get; set; }
		[ProtoMember(2)]
		public string Password { get; set; }
		[ProtoMember(3)]
		public Guid PasswordToken { get; set; }
		[ProtoMember(4)]
        public IList<string> Organisations { get; set; }

		[ProtoMember(5), Obsolete("Now stored in Organisations list to support multiple orgs per user")]
		public string OrganisationId { get; set; }
    }
}