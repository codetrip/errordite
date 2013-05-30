
using System.Collections.Generic;
using Errordite.Core.Extensions;
using Errordite.Core.Domain.Error;

namespace Errordite.Web.Models.Errors
{
    public class ExceptionViewModel
	{
		public Error Error { get; set; }
        public ExceptionInfo Info { get; set; }
		public List<ExtraDataItemViewModel> ExtraData { get; set; }
		public bool InnerException { get; set; }

        public ExceptionViewModel(ExceptionInfo info, Error error, bool innerException, List<ExtraDataItemViewModel> extraData)
        {
            Info = info;
			Error = error;
            ExtraData = extraData;
	        InnerException = innerException;
        }
        
        public bool DisplayInfoTable()
		{
			if (InnerException)
			{
				return Info.MethodName.IsNotNullOrEmpty() || Info.Module.IsNotNullOrEmpty() || (Info.ExtraData != null && Info.ExtraData.Count > 0);
			}

			return 
				Info.MethodName.IsNotNullOrEmpty() || 
				Info.Module.IsNotNullOrEmpty() || 
				(Info.ExtraData != null && Info.ExtraData.Count > 0) || 
				(Error.ContextData != null && Error.ContextData.Count > 0);
		}
    }

    public class ExtraDataItemViewModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool CanMakeRule { get; set; }
    }
}