
using System;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Exceptions;
using CodeTrip.Core.Dynamic;

namespace CodeTrip.Core
{
    /// <summary>
    /// Base class for all components, exposes methods for instrumentation, including, trace, error and audit
    /// </summary>
    public abstract class ComponentBase : ISupportsAuditing
    {
        public IComponentAuditor Auditor { get; set; }

        protected void AssertAuditor()
        {
            if (Auditor == null)
                throw new CodeTripDependentComponentException<IComponentAuditor>();
        }

        public void Trace(string message, params object[] args)
        {
            AssertAuditor();
            Auditor.Trace(GetType(), message, args);
        }

        public void Info(string message, params object[] args)
        {
            AssertAuditor();
            Auditor.Info(GetType(), message, args);
        }

        public void Warning(string message, params object[] args)
        {
            AssertAuditor();
            Auditor.Warning(GetType(), message, args);
        }

        public void Error(Exception e)
        {
            AssertAuditor();
            Auditor.Error(GetType(), e);
        }

        public void Error(string message, params object[] args)
        {
            AssertAuditor();
            Auditor.Error(GetType(), message, args);
        }

        public void Error(Exception e, int eventId)
        {
            AssertAuditor();
            Auditor.Error(GetType(), eventId, e);
        }

        public void Error(Exception e, Guid messageId)
        {
            AssertAuditor();
            Auditor.Error(GetType(), e, messageId);
        }

        /// <summary>
        /// Uses the SummaryWriter to (efficiently) output an object to the trace.
        /// </summary>
        public void TraceObject<T>(T obj) where T : class
        {
            var type = obj == null ? typeof(T) : obj.GetType();
            Auditor.Trace(type, SummaryWriter.GetSummary(obj));
        }
    }
}