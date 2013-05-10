
using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
	[Serializable]
	public class ErrorditeConfigurationException : ErrorditeException
    {
        public ErrorditeConfigurationException()
        {}

        public ErrorditeConfigurationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeConfigurationException(string message)
            : base (message)
        {}

        public ErrorditeConfigurationException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeConfigurationException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}