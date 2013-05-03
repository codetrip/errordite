using System;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Messaging
{
    public class MessageEnvelope
    {
        public string Id { get; set; }
        public DateTime GeneratedOnUtc { get; set; }
		public string Message { get; set; }
		public string ErrorMessage { get; set; }
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string OrganisationId { get; set; }
        public string QueueUrl { get; set; }
		public string ReceiptHandle { get; set; }
        public Service Service { get; set; }

		[JsonIgnore]
		public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

		public static string GetId(string friendlyId)
		{
			return friendlyId.Contains("/") ? friendlyId : "MessageEnvelopes/{0}".FormatWith(friendlyId);
		}
    }
}
