using System;
using System.Text;
using System.Text.RegularExpressions;
using Errordite.Core.Auditing.Entities;
using Errordite.Core.Exceptions;

namespace Errordite.Core.Auditing.Auditors
{
    public abstract class ComponentAuditorBase : IComponentAuditor
    {
        private void CommonAuditEventPopulation(Type component, AuditEvent auditEvent)
        {
            auditEvent.Component = component.Name;
            auditEvent.Timestamp = DateTime.UtcNow;
            auditEvent.ApplicationName = LoggerName;
        }

        private AuditEvent CreateEventForException(Type component, Exception exception, int eventId = 0, Guid? messageId = null)
        {
            var auditEvent = new ExceptionAuditEvent
            {
                Error = exception,
                EventId = eventId,
                MessageId = messageId
            };

            CommonAuditEventPopulation(component, auditEvent);

            auditEvent.Message = exception.Message;
            auditEvent.FormattedMessage = ExceptionUtility.FormatException(exception, true, true, true);

            return auditEvent;
        }


        /// <summary>
        /// This should create a new instance of an <see cref="AuditEvent"/> 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventId"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private AuditEvent CreateEvent(Type component, int eventId, string message, params object[] args)
        {
            string formattedMessage;

            try
            {
                formattedMessage = string.Format(message, args);
            }
            catch (Exception e)
            {
                //make sure there are no format markers in the message
                formattedMessage = Regex.Replace(message, "{([0-9]*)}", string.Empty);

                // if we get an exception when creating the AuditEvent it is because string.format()
                // blew up.  Hence we want to audit the message with no parameters (so that something gets audited)
                // and also produce an error so we can fix this in the future.
                var argText = new StringBuilder();
                foreach (var arg in args)
                    argText.Append(arg).Append('|');

                Error(component, new ErrorditeAuditException("Exception when creating AuditEvent. Message: " + formattedMessage + " Args: " + argText, true, e));
            }

            var auditEvent = new AuditEvent
            {
                Message = formattedMessage,
                EventId = eventId,
                FormattedMessage = ModuleName == null ? 
                    string.Format("[{0}] {1}", component.Name, formattedMessage) :
                    string.Format("[{0}::{1}] {2}", ModuleName, component.Name, formattedMessage)
            };

            CommonAuditEventPopulation(component, auditEvent);

            return auditEvent;
        }

        protected abstract void DoTrace(AuditEvent auditEvent);
        protected abstract void DoInfo(AuditEvent auditEvent);
        protected abstract void DoWarning(AuditEvent auditEvent);
        protected abstract void DoError(AuditEvent auditEvent);

        public abstract bool IsDebugEnabled { get; }

        #region Implementation of IComponentAuditor

        public string LoggerName { get; set; }
        public string ModuleName { get; set; }

        public void Trace(Type component, string message, params object[] args)
        {
            DoTrace(CreateEvent(component, 0, message, args));
        }

        public void Info(Type component, string message, params object[] args)
        {
            DoInfo(CreateEvent(component, 0, message, args));
        }

        public void Warning(Type component, string message, params object[] args)
        {
            DoWarning(CreateEvent(component, 0, message, args));
        }

        public void Error(Type component, string message, params object[] args)
        {
            Error(component, 0, message, args);
        }

        public void Error(Type component, int eventId, string message, params object[] args)
        {
            DoError(CreateEvent(component, eventId, message, args));
        }

        public void Error(Type component, Exception e)
        {
            Error(component, 0, e);
        }

        public void Error(Type component, Exception e, Guid messageId)
        {
            DoError(CreateEventForException(component, e, 0, messageId));
        }

        public void Error(Type component, int eventId, Exception e)
        {
            DoError(CreateEventForException(component, e, eventId));
        }

        #endregion
    }
}
