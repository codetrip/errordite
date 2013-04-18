
using Errordite.Core.Messages;

namespace Errordite.Services.Entities
{
    public class MessageEnvelope
    {
        public MessageBase Message { get; set; }
        public string ReceiptHandle { get; set; }
    }
}
