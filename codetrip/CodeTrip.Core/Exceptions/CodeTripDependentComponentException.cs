
namespace CodeTrip.Core.Exceptions
{
    #region Directives
    using System;
    using System.Runtime.Serialization;
    #endregion

	[Serializable]
	public class CodeTripDependentComponentException<T> : CodeTripException
    {
        public CodeTripDependentComponentException()
            : base(DefaultMessage())
        { }

        public CodeTripDependentComponentException(bool logged)
            : base(DefaultMessage(), logged)
        { }

        public CodeTripDependentComponentException(string componentName)
            : base(DefaultMessage(componentName))
        {}

        public CodeTripDependentComponentException(bool logged, string componentName)
            : base(DefaultMessage(componentName), logged)
        { }


        public CodeTripDependentComponentException(SerializationInfo info, StreamingContext context) 
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