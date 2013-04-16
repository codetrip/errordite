using System;
using CodeTrip.Core.Auditing.Entities;

namespace CodeTrip.Core.Auditing.Auditors
{
    public class DelegateComponentAuditor : ComponentAuditorBase
    {
        private readonly Action<string> _loggingDelegate;

        public DelegateComponentAuditor(Action<string> loggingDelegate)
        {
            _loggingDelegate = loggingDelegate;
        }

        protected override void DoTrace(AuditEvent auditEvent)
        {
            _loggingDelegate.Invoke(auditEvent.FormattedMessage);
        }

        protected override void DoInfo(AuditEvent auditEvent)
        {
            _loggingDelegate.Invoke(auditEvent.FormattedMessage);
        }

        protected override void DoWarning(AuditEvent auditEvent)
        {
            _loggingDelegate.Invoke(auditEvent.FormattedMessage);
        }

        protected override void DoError(AuditEvent auditEvent)
        {
            _loggingDelegate.Invoke(auditEvent.FormattedMessage);
        }

        public override bool IsDebugEnabled
        {
            get { return true; }
        }
    }
}