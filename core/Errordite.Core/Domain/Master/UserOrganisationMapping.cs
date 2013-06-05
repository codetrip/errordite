using System;
using System.Collections.Generic;
using Errordite.Core.Domain.Organisation;
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
		[ProtoMember(5)]
		public UserStatus Status { get; set; }

        public bool SsoUser { get; set; }
    }
}