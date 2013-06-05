using System;
using System.Collections.Generic;

namespace Errordite.Core.Domain.Error
{
	public class IssueHourlyCount
	{
		public string Id { get; set; }
		public string IssueId { get; set; }
		public string ApplicationId { get; set; }
		public Dictionary<int, int> HourlyCount { get; set; }

		public void IncrementHourlyCount(DateTimeOffset time)
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