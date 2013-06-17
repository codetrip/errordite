using System;
using System.Collections.Generic;
using Errordite.Core.Paging;
using Errordite.Core.Domain.Error;
using App = Errordite.Core.Domain.Organisation;
using System.Linq;

namespace Errordite.Web.Models.Errors
{
    public class ErrorPageViewModel
    {
        public ErrorCriteriaViewModel ErrorsViewModel { get; set; }
        public bool NoApplications { get; set; }
    }

    public class ErrorCriteriaViewModel : ErrorCriteriaPostModel
    {
        public bool HideIssues { get; set; }
        public IList<ErrorInstanceViewModel> Errors { get; set; }
        public string Sort { get; set; }
		public bool SortDescending { get; set; }
		public bool ReadOnly { get; set; }
    }

    public class ErrorInstanceViewModel
    {
        public ErrorInstanceViewModel()
        {
            PropertiesEligibleForRules = new List<string>();
        }

        public Error Error { get; set; }
        public bool HideIssues { get; set; }
		public string ApplicationName { get; set; }
		public bool IsGetMethod { get; set; }
        public IEnumerable<ExceptionViewModel> Exceptions 
		{
			get
			{
				return Error.ExceptionInfos
							.Select((info, i) => new ExceptionViewModel(info, Error, i > 0,
								(info.ExtraData ?? new Dictionary<string, string>())
									.Select(
										kvp => new ExtraDataItemViewModel
											{
												Key = kvp.Key,
												Value = kvp.Value,
												CanMakeRule = PropertiesEligibleForRules.Contains(kvp.Key),
											}).ToList()));
			}
		}

        public List<string> PropertiesEligibleForRules { get; set; }

    }

    [Serializable]
    public class ErrorCriteriaPostModel
    {
        public string Id { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Query { get; set; }
        public string DateRange { get; set; }
        public PagingViewModel Paging { get; set; }
    }
}