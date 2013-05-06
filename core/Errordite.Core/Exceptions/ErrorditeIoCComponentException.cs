using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
    [Serializable]
    public class ErrorditeIoCComponentException : ErrorditeException
    {
        public ErrorditeIoCComponentException()
        {}

        public ErrorditeIoCComponentException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeIoCComponentException(string message)
            : base(message)
        {}

        public ErrorditeIoCComponentException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeIoCComponentException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        public static string DefaultMessage(string componentName)
        {
            return string.Format("A problem has occured while attempting to resolve component of type '{0}'", componentName);
        }
    }

    public class ErrorditeIoCComponentException<T> : ErrorditeIoCComponentException
    {
        public ErrorditeIoCComponentException()
            : base(DefaultMessage())
        { }

        public ErrorditeIoCComponentException(Exception innerException)
            : this(DefaultMessage(), false, innerException)
        { }

        public ErrorditeIoCComponentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public ErrorditeIoCComponentException(string message)
            : base(message)
        { }

        public ErrorditeIoCComponentException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeIoCComponentException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string DefaultMessage()
        {
            // this string can be replaced by locale sensitive 
            // resource entries or something flash liks this later on
            return string.Format("A component of type '{0}' could not be located in the IoC container; this indicates a configuration or deployment problem",
                typeof(T).Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        public static new string DefaultMessage(string componentName)
        {
            // this string can be replaced by locale sensitive 
            // resource entries or something flash liks this later on
            return string.Format("A component of type '{0}' with object key '{1}' could not be located in the IoC container; this indicates a configuration or deployment problem",
                typeof(T).Name,
                componentName);
        }
    }
}