using System;
using System.Collections.Generic;
using System.Reflection;
using System.Transactions;
using CodeTrip.Core.Auditing;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using Raven.Client;
using log4net.Appender;
using log4net.Core;

namespace Errordite.Core.Auditing
{
    public class RavenExceptionAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var auditEvent = loggingEvent.MessageObject as ExceptionAuditEvent ?? new ExceptionAuditEvent
                {
                    Error = loggingEvent.ExceptionObject ?? new NoException(),
                    FormattedMessage = loggingEvent.ExceptionObject != null ?
                        ExceptionUtility.FormatException(loggingEvent.ExceptionObject, true, true, true) :
                        loggingEvent.RenderedMessage
                };

                var errorditeError = GetError(auditEvent);

                //suppress any ambient transaction so this session is commited
                using(var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    //need to create a new session for errors so we dont commit changes from the request session
                    using(var session = ObjectFactory.GetObject<IDocumentStore>().OpenSession(CoreConstants.ErrorditeMasterDatabaseName))
                    {
                        session.Store(errorditeError);
                        session.SaveChanges();
                        transaction.Complete();
                    }
                }

                //always write the formatted excpetion to the debug trace
                System.Diagnostics.Trace.Write(auditEvent.FormattedMessage);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("Unhandled exception in RavenExceptionAppender");
                System.Diagnostics.Trace.Write(e.ToString());
            }
        }

        private ErrorditeError GetError(ExceptionAuditEvent auditEvent)
        {
            var errorditeError = new ErrorditeError
            {
                TimestampUtc = DateTime.UtcNow,
                Machine = Environment.MachineName,
                Text = auditEvent.FormattedMessage,
                Type = auditEvent.Error.GetType().FullName,
                Message = auditEvent.Error.Message,
                Data = new Dictionary<string, string>(),
                Module = auditEvent.Error.Source,
                Application = auditEvent.ApplicationName,
                MessageId = auditEvent.MessageId
            };

            MethodBase method = auditEvent.Error.TargetSite;
            if (method != null)
            {
                errorditeError.Method = string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name);
            }

            foreach (var key in auditEvent.Error.Data.Keys)
            {
                errorditeError.Data.Add(key.ToString(), auditEvent.Error.Data[key].ToString());

                if (key.ToString() == CoreConstants.ExceptionKeys.User)
                {
                    errorditeError.User = auditEvent.Error.Data[key].ToString();
                }
            }

            return errorditeError;
        }
    }
}
