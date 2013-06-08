using System;

namespace Errordite.Core.Domain.Error
{
	public class IssueDailyCount
	{
		public string Id { get; set; }
        public string IssueId { get; set; }
        public string ApplicationId { get; set; }
        public int Count { get; set; }
		public bool Historical { get; set; }
        public DateTime Date { get; set; }
	}
}
