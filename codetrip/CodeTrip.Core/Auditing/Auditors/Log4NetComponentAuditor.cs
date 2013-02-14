
using System;

namespace CodeTrip.Core.Auditing.Auditors
{
    using Entities;

    public class Log4NetComponentAuditor : ComponentAuditorBase
    {
        private readonly log4net.ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetComponentAuditor"/> class.
        /// </summary>
        public Log4NetComponentAuditor(string loggerName)
        {
            _log = log4net.LogManager.GetLogger(loggerName);
            LoggerName = loggerName;
        }

        protected override void DoTrace(AuditEvent auditEvent)
        {
            _log.Debug(auditEvent.FormattedMessage);
        }

        protected override void DoInfo(AuditEvent auditEvent)
        {
            _log.Info(auditEvent.FormattedMessage);
        }

        protected override void DoWarning(AuditEvent auditEvent)
        {
            _log.Warn(auditEvent.FormattedMessage);
        }

        protected override void DoError(AuditEvent auditEvent)
        {
            _log.Error(auditEvent);
        }

        public override bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }
    }
}