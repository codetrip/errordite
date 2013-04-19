using System;
using System.Collections.Generic;
using System.Messaging;
using System.Transactions;
using System.Linq;
using Errordite.Core;
using Errordite.Core.Interfaces;

namespace Errordite.Core.Monitoring.Commands
{
    public class DeleteMessagesCommand : ComponentBase, IDeleteMessagesCommand
    {
        public DeleteMessagesResponse Invoke(DeleteMessagesRequest request)
        {
            Trace("Starting...");

            var filter = new MessagePropertyFilter {Id = true};

            using (var errorQueue = new MessageQueue(request.ErrorQueue) { Formatter = new XmlMessageFormatter(), MessageReadPropertyFilter = filter })
            {
                if (request.MessageIds == null || !request.MessageIds.Any())
                {
                    errorQueue.Purge();
                }
                else
                {
                    foreach (var messageId in request.MessageIds)
                    {
                        try
                        {
                            var tt = errorQueue.Transactional ? Transaction.Current == null
                                ? MessageQueueTransactionType.Single
                                : MessageQueueTransactionType.Automatic
                                    : MessageQueueTransactionType.None;

                            var msg = errorQueue.ReceiveById(messageId, TimeSpan.FromSeconds(5.0), tt);

                            if (msg != null)
                                Trace("Deleting message with Id:={0}", msg.Id);
                        }
                        catch (MessageQueueException ex)
                        {
                            if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                                throw;
                        }
                    }
                }
            }

            return new DeleteMessagesResponse();
        }
    }

    public interface IDeleteMessagesCommand : ICommand<DeleteMessagesRequest, DeleteMessagesResponse>
    { }

    public class DeleteMessagesResponse 
    {}

    public class DeleteMessagesRequest 
    {
        public string ErrorQueue { get; set; }
        public IEnumerable<string> MessageIds { get; set; }
    }
}
