using Errordite.Core;
using Errordite.Core.Messages;

namespace Errordite.Services.Consumers
{
    public class ReceiveErrorConsumer : ComponentBase, IErrorditeConsumer<ErrorReceivedMessage>
    {
        public void Consume(ErrorReceivedMessage message)
        {
            TraceObject(message);
        }
    }
}