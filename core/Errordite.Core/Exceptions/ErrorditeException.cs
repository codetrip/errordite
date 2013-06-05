
using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
	[Serializable]
	public abstract class ErrorditeException : Exception
    {
        /// <summary>
        /// Flag indicating whether the exception has been logged
        /// </summary>
        public bool Logged { get; set; }

        protected ErrorditeException()
        {}

        protected ErrorditeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        protected ErrorditeException(string message)
            : this(message, false)
        {}

        protected ErrorditeException(string message, bool logged)
            : base (message)
        {
            Logged = logged;
        }

        protected ErrorditeException(string message, bool logged, Exception innerException)
            : base (message, innerException)
        {
            Logged = logged;
        }
    }
}