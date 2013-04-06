using System;
using System.Diagnostics;
using System.Threading;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;
using Errordite.Client;
using Errordite.Core.Session;
using NServiceBus;
using Raven.Abstractions.Exceptions;

namespace Errordite.Core.ServiceBus
{
    /// <summary>
    /// This doesn't really do anything "session"-y  except set the RequestLimit.  We could easily get rid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MessageHandlerSessionBase<T> : SessionAccessBase, IHandleMessages<T> where T : ErrorditeNServiceBusMessageBase
    {
        protected abstract void HandleMessage(T message);

        protected virtual string GetAuditMessage(T message, Stopwatch watch)
        {
            return Resources.CoreResources.AsyncJobProcessed.FormatWith(watch.Elapsed.Seconds, watch.Elapsed.Milliseconds);
        }

        public void Handle(T message)
        {
            Trace("Received Message of type {0}", message.GetType().FullName);
            TraceObject(message);

            var watch = Stopwatch.StartNew();

            try
            {
                //dont want a limit on session requests for offline processes
                Session.RequestLimit = int.MaxValue;

                HandleMessage(message);
                Trace("Processed message in {0}ms".FormatWith(watch.ElapsedMilliseconds));
            }
            catch (Exception e)
            {
                //if the error is a concurrency exception wait 250ms to force a retry delay (not supported by NServiceBus)
                if (e is ConcurrencyException)
                {
                    Thread.Sleep(250);    
                }

                e.Data.Add("MessageType", typeof(T).Name); 

                try
                {
                    e.Data.Add("Message", SerializationHelper<T>.DataContractJsonSerialize(message));
                }
                catch { }

                ErrorditeClient.ReportException(e, false);
                Error(e, message.Id);
                throw;
            }
        }
    }

    public abstract class MessageHandlerBase<T> : ComponentBase, IHandleMessages<T> where T : class, IMessage
    {
        protected abstract void HandleMessage(T message);

        public void Handle(T message)
        {
            Trace("Received Message of type {0}", message.GetType().FullName);
            TraceObject(message);

            var watch = Stopwatch.StartNew();

            try
            {
                HandleMessage(message);
                Trace("Processed message in {0}ms".FormatWith(watch.ElapsedMilliseconds));
            }
            catch (Exception e)
            {
                e.Data.Add("MessageType", typeof(T).Name); 

                try
                {
                    e.Data.Add("Message", SerializationHelper<T>.DataContractJsonSerialize(message));
                }
                catch{}
                
                ErrorditeClient.ReportException(e, false);
                Error(e);
                throw;
            }
        }
    }
}
