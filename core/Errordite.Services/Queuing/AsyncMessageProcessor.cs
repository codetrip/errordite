using System.Collections.Generic;
using Errordite.Core.Messages;

namespace Errordite.Services.Queuing
{
    public class AsyncMessageProcessor
    {
        private readonly Queue<MessageBase> _internalQueue = new Queue<MessageBase>();
    }
}
