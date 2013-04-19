using System;
using System.Messaging;
using Errordite.Core;
using Errordite.Core.Interfaces;
using Errordite.Core.Monitoring.Entities;

namespace Errordite.Core.Monitoring.Queries
{
    public class GetQueueStatusQuery : ComponentBase, IGetQueueStatusQuery
    {
        public GetQueueStatusResponse Invoke(GetQueueStatusRequest request)
        {
            return new GetQueueStatusResponse
            {
                Status = GetQueueStatus(request.QueuePath)
            };
        }

        private QueueStatus GetQueueStatus(string queueName)
        {
            var status = new QueueStatus
            {
                QueueName = queueName
            };

            if (MessageQueue.Exists(queueName))
            {
                var queue = new MessageQueue(queueName)
                {
                    MessageReadPropertyFilter = new MessagePropertyFilter
                    {
                        ArrivedTime = true
                    }
                };

                const int MaxQueueLengthLimit = 10000;
                DateTime? earliestMessage;
                var count = GetMessageCount(queue, MaxQueueLengthLimit, out earliestMessage);

                status.EarliestMessage = earliestMessage;
                status.TotalMessages = count;
            }
            else
            {
                status.Message = "Queue does not exist";
            }

            return status;
        }

        private Message PeekWithoutTimeout(MessageQueue q, Cursor cursor, PeekAction action)
        {
            Message ret = null;
            try
            {
                ret = q.Peek(new TimeSpan(1), cursor, action);
            }
            catch (MessageQueueException mqe)
            {
                if (!mqe.Message.ToLower().Contains("timeout"))
                {
                    throw;
                }
            }
            return ret;
        }

        private int GetMessageCount(MessageQueue q, int limit, out DateTime? earliestMessage)
        {
            int count = 0;
            Cursor cursor = q.CreateCursor();
            earliestMessage = null;
            Message m = PeekWithoutTimeout(q, cursor, PeekAction.Current);
            if (m != null)
            {
                count = 1;
                while ((m = PeekWithoutTimeout(q, cursor, PeekAction.Next)) != null && count <= limit)
                {
                    if (earliestMessage == null)
                        earliestMessage = m.ArrivedTime;

                    count++;
                }
            }
            return count;
        }
    }

    public interface IGetQueueStatusQuery : IQuery<GetQueueStatusRequest, GetQueueStatusResponse>
    { }

    public class GetQueueStatusResponse
    {
        public QueueStatus Status { get; set; }
    }

    public class GetQueueStatusRequest
    {
        public string QueuePath { get; set; }
    }
}
