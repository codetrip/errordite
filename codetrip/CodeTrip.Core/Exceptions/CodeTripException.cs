
using System;
using System.Runtime.Serialization;

namespace CodeTrip.Core.Exceptions
{
	[Serializable]
	public abstract class CodeTripException : Exception
    {
        /// <summary>
        /// Flag indicating whether the exception has been logged
        /// </summary>
        public bool Logged { get; set; }

        protected CodeTripException()
        {}

        protected CodeTripException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        protected CodeTripException(string message)
            : this(message, false)
        {}

        protected CodeTripException(string message, bool logged)
            : base (message)
        {
            Logged = logged;
        }

        protected CodeTripException(string message, bool logged, Exception innerException)
            : base (message, innerException)
        {
            Logged = logged;
        }
    }
}