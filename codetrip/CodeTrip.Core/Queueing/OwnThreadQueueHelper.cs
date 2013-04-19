using System;
using System.Collections.Concurrent;
using System.Threading;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;

namespace CodeTrip.Core.Queueing
{
    /// <summary>
    /// Acts as a wrapper around a queue and a worker thread.  The queue can be added to 
    /// or cleared and the worker thread processes the items, calling the action passed
    /// in on the ctor.
    /// </summary>
    /// <typeparam name="T">The type of the queue item.</typeparam>
    public sealed class OwnThreadQueueHelper<T> where T : class
    {
        private readonly Thread _workerThread;
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly Action<T> _action;
        private readonly EventWaitHandle _waitHandle = new AutoResetEvent(false);

        public OwnThreadQueueHelper(Action<T> action)
        {
            _action = action;
            _workerThread = new Thread(ProcessOutgoing)
            {
                IsBackground = true
            };
            _workerThread.Start();
        }

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);
            _waitHandle.Set();
        }

        private void ProcessOutgoing()
        {
            while (true)
            {
                T obj;
                if (!_queue.TryDequeue(out obj))
                {
                    //if data was not found (meaning list is empty) then wait till something is added to it
                    _waitHandle.WaitOne();
                }

                if (obj != null)
                {
                    try
                    {
                        _action(obj);
                    }
                    catch (Exception ex)
                    {
                        ObjectFactory.GetObject<IComponentAuditor>().Error(GetType(), ex);
                    }
                }
            }
        }

        public int GetQueueLength(out T firstItem)
        {
            return _queue.TryPeek(out firstItem) ? _queue.Count : 0;
        }

        public void ClearQueue()
        {
            _queue = new ConcurrentQueue<T>();
        }
    }
}