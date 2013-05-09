using System;
using System.Collections;
using System.Collections.Generic;
using System.Messaging;
using System.Transactions;
using System.Linq;
using Errordite.Core;
using Errordite.Core.Interfaces;

namespace Errordite.Core.Monitoring.Commands
{
    public class ReturnMessageToSourceQueueCommand : ComponentBase, IReturnMessageToSourceQueueCommand
    {
        public ReturnMessageToSourceQueueResponse Invoke(ReturnMessageToSourceQueueRequest request)
        {
            var filter = new MessagePropertyFilter {Id = true};

            using (var errorQueue = new MessageQueue(request.ErrorQueue) { Formatter = new XmlMessageFormatter(), MessageReadPropertyFilter = filter })
            {
                using (var queue = new MessageQueue(request.SourceQueue))
                {
                    if (request.MessageIds == null || !request.MessageIds.Any())
                    {
                        var messages = errorQueue.GetAllMessages();

                        foreach(var id in messages.Select(m => m.Id).ToList())
                        {
                            try
                            {
                                var tt = errorQueue.Transactional ? Transaction.Current == null
                                    ? MessageQueueTransactionType.Single
                                    : MessageQueueTransactionType.Automatic
                                        : MessageQueueTransactionType.None;

                                var message = errorQueue.ReceiveById(id, TimeSpan.FromSeconds(5.0), tt);

                                if (message != null)
                                {
                                    queue.Send(message, tt);
                                }
                            }
                            catch (MessageQueueException ex)
                            {
                                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                                    throw;
                            }
                        }
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

                                var message = errorQueue.ReceiveById(messageId, TimeSpan.FromSeconds(5.0), tt);

                                if (message != null)
                                {
                                    queue.Send(message, tt);
                                }
                            }
                            catch (MessageQueueException ex)
                            {
                                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                                    throw;
                            }
                        }
                    }
                }
            }

            return new ReturnMessageToSourceQueueResponse();
        }
    }

    public interface IReturnMessageToSourceQueueCommand : ICommand<ReturnMessageToSourceQueueRequest, ReturnMessageToSourceQueueResponse>
    { }

    public class ReturnMessageToSourceQueueResponse 
    {}

    public class ReturnMessageToSourceQueueRequest 
    {
        public string SourceQueue { get; set; }
        public string ErrorQueue { get; set; }
        public IEnumerable<string> MessageIds { get; set; }
    }
}
