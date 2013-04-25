using System;
using Errordite.Core.Configuration;

namespace Errordite.Core.Messaging
{
    public class MessageEnvelope
    {
        public string Id { get; set; }
        public DateTime GeneratedOnUtc { get; set; }
        public string Message { get; set; }
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string OrganisationId { get; set; }
        public string QueueUrl { get; set; }
        public string ReceiptHandle { get; set; }
        public Service Service { get; set; }
    }
}
