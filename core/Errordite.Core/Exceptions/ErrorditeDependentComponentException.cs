
namespace Errordite.Core.Exceptions
{
    #region Directives
    using System;
    using System.Runtime.Serialization;
    #endregion

	[Serializable]
	public class ErrorditeDependentComponentException<T> : ErrorditeException
    {
        public ErrorditeDependentComponentException()
            : base(DefaultMessage())
        { }

        public ErrorditeDependentComponentException(bool logged)
            : base(DefaultMessage(), logged)
        { }

        public ErrorditeDependentComponentException(string componentName)
            : base(DefaultMessage(componentName))
        {}

        public ErrorditeDependentComponentException(bool logged, string componentName)
            : base(DefaultMessage(componentName), logged)
        { }


        public ErrorditeDependentComponentException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}
        
        private static string DefaultMessage()
        {
            // this string can be replaced by locale sensitive 
            // resource entries or something flash liks this later on
            return string.Format("A dependent component of type '{0}' is null or otherwise invalid; execution is aborted",
                typeof(T).Name);
        }

        private static string DefaultMessage(string componentName)
        {
            // this string can be replaced by locale sensitive 
            // resource entries or something flash liks this later on
            return string.Format("Dependent component of type '{0}' and name '{1}' is invalid; execution is aborted", 
                typeof(T).Name,
                componentName);
        }
    }
}