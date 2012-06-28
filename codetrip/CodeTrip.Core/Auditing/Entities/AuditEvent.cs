using System;

namespace CodeTrip.Core.Auditing.Entities
{
    public class ExceptionAuditEvent : AuditEvent
    {
        public Exception Error { get; set; }
    }

    public class AuditEvent
    {
        /// <summary>
        /// The application name
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The component that this event is associated with
        /// </summary>
		public string Component { get; set; }

        /// <summary>
        /// The datetime that this event occurred (usually utc)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The message to record
        /// </summary>
        public string Message { get; set; }

		/// <summary>
		/// The formatted message to record
		/// </summary>
		public string FormattedMessage { get; set; }

        /// <summary>
        /// An EventId property to associate with the audit
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// NServiceBus message Id for correlation
        /// </summary>
        public Guid? MessageId { get; set; }
    }
}