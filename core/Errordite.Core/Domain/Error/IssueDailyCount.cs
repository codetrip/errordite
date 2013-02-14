using System;
using System.Collections.Generic;

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

	public class IssueHourlyCount
	{
		public string Id { get; set; }
		public string IssueId { get; set; }
		public Dictionary<int, int> HourlyCount { get; set; }

		public void IncrementHourlyCount(DateTime time)
		{
			HourlyCount[time.Hour]++;
		}

		public void Initialise()
		{
			HourlyCount = new Dictionary<int, int>();
			for(int index = 0;index< 24;index++)
			{
				HourlyCount.Add(index, 0);
			}
		}
	}
}
