using System;

namespace Errordite.Core.Extensions
{
    public static class DateTimeExtensions
    {
	    private static DateTime _epoch = new DateTime(1970, 1, 1);

        public static DateTime RangeEnd(this DateTime date)
        {
            //this is designed for use in an "end of range" search; i.e. if you have specified the date
            //24 May 2012, what you actually mean is the end of that day, however if you've specified a
            //time, you actually mean that time
            if (date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                return date.AddDays(1);

            return date;
		}

		public static double ConvertToUnixTimestamp(this DateTime date)
		{
			var d2 = date.ToUniversalTime();
			var ts = new TimeSpan(d2.Ticks - _epoch.Ticks);
			return ts.TotalMilliseconds;
		}

        public static string ToLocalTimeFormatted(this DateTimeOffset datetimeUtc)
        {
            return datetimeUtc.ToString("dd MMM yyyy HH:mm:ss");
        }

        public static string ToLocalFormatted(this DateTimeOffset datetimeUtc)
        {
			return datetimeUtc.ToString("dd MMM yyyy");
        }

        public static string ToLocalTimeFormatted(this DateTime datetimeUtc)
        {
            return datetimeUtc.ToLocalTime().ToString("dd MMM yyyy HH:mm:ss");
        }

        public static string ToLocalFormatted(this DateTime datetimeUtc)
        {
            return datetimeUtc.ToLocalTime().ToString("dd MMM yyyy");
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime datetimeUtc, string timeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? "UTC");
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(datetimeUtc, timeZone);
            var utcOffset = timeZone.GetUtcOffset(datetimeUtc);
            return new DateTimeOffset(localDate, utcOffset).ToLocalTime(); //use ToLocatTime here so its adjusted for daylight savings
        }

        /// <summary>
        /// Converts a DateTime compared to the current time into plain english for explaining how recently the datetime occurred in the past.
        /// </summary>
        public static string ToVerbalTimeSinceUtc(this DateTimeOffset sourceDateTime, string timezoneId, bool formatWithHtml = false)
        {
            var timeSpan = DateTime.UtcNow.ToDateTimeOffset(timezoneId).Subtract(sourceDateTime);

            string verbalResult;

            if (timeSpan.TotalDays / 365 >= 1)
            {
                var year = (int)Math.Round(timeSpan.TotalDays / 365);
                var yearString = (year == 1) ? "year ago" : "years ago";
                verbalResult = FormatResult(formatWithHtml, year, yearString);
            }
            else if (timeSpan.TotalDays / 30 >= 1)
            {
                var month = (int)Math.Round(timeSpan.TotalDays / 30);
                var monthString = (month == 1) ? "month ago" : "months ago";
                verbalResult = FormatResult(formatWithHtml, month, monthString);
            }
            else if (timeSpan.TotalDays / 7 >= 1)
            {
                var week = (int)Math.Round(timeSpan.TotalDays / 7);
                var weekString = (week == 1) ? "week ago" : "weeks ago";
                verbalResult = FormatResult(formatWithHtml, week, weekString);
            }
            else if (timeSpan.Days > 1)
            {
                verbalResult = FormatResult(formatWithHtml, timeSpan.Days, "days ago");
            }
            else if (timeSpan.Days == 1)
            {
                verbalResult = FormatResult(formatWithHtml, "yesterday");
            }
            else if (timeSpan.Hours >= 1)
            {
                var hourString = (timeSpan.Hours == 1) ? "hour ago" : "hours ago";
                verbalResult = FormatResult(formatWithHtml, timeSpan.Hours, hourString);
            }
            else if (timeSpan.Minutes >= 1)
            {
                var minuteString = (timeSpan.Minutes == 1) ? "minute ago" : "minutes ago";
                verbalResult = FormatResult(formatWithHtml, timeSpan.Minutes, minuteString);
            }
            else
            {
                if (timeSpan.Seconds == 0)
                {
                    verbalResult = FormatResult(formatWithHtml, "a moment ago");
                }
                else
                {
                    var secondsString = (timeSpan.Seconds == 1) ? "second" : "seconds";
                    verbalResult = FormatResult(formatWithHtml, timeSpan.Seconds, secondsString);
                }
            }

            return !string.IsNullOrEmpty(verbalResult) ? verbalResult : sourceDateTime.ToLocalTimeFormatted();
        }

        private static string FormatResult(bool formatWithHtml, double? number, string interval)
        {
            return formatWithHtml
                ? string.Format("<span class='number'>{0}</span> <span clas='interval'>{1}</span>", number, interval)
                : string.Format("{0} {1}", number, interval);
        }

        private static string FormatResult(bool formatWithHtml, string interval)
        {
            return formatWithHtml
                ? string.Format("<span clas='interval'>{0}</span>", interval)
                : interval;
        }
    }
}
