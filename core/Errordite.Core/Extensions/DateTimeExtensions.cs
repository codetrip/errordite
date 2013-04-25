using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Errordite.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime RangeEnd(this DateTime date)
        {
            //this is designed for use in an "end of range" search; i.e. if you have specified the date
            //24 May 2012, what you actually mean is the end of that day, however if you've specified a
            //time, you actually mean that time
            if (date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                return date.AddDays(1);

            return date;
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
            return datetimeUtc.ToString("dd MMM yyyy HH:mm:ss");
        }

        public static string ToLocalFormatted(this DateTime datetimeUtc)
        {
            return datetimeUtc.ToString("dd MMM yyyy");
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime datetimeUtc, string timeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? "UTC");
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(datetimeUtc, timeZone);
            var utcOffset = timeZone.GetUtcOffset(datetimeUtc);
            return new DateTimeOffset(localDate, utcOffset);
        }

		public static string ExpressedAsTimePeriod(this DateTime utcDateTime)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            var ts = DateTime.UtcNow - utcDateTime;

            if (ts.TotalHours < 1)
                return ts.Minutes == 0
                    ? string.Format("{0} second{1} ago", ts.Seconds, ts.Seconds == 1 ? "" : "s")
                    : string.Format("{0} minute{1} ago", ts.Minutes, ts.Minutes == 1 ? "" : "s");

            return ts.TotalDays < 1
                ? string.Format("{0} hour{1} ago", ts.Hours, ts.Hours == 1 ? "" : "s")
                : utcDateTime.ToLocalTime().ToString("dd MMM yyyy HH:mm");
        }

        public static MvcHtmlString ExpressedAsTimePeriodHtml(this DateTime utcDateTime)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            var ts = DateTime.UtcNow - utcDateTime;

            string result;

            if (ts.TotalHours < 1)
            {
                result = ts.Minutes == 0
                    ? string.Format("<span class='number'>{0}</span> <span class='interval'>second{1} ago</span>", ts.Seconds, ts.Seconds == 1 ? "" : "s")
                    : string.Format("<span class='number'>{0}</span> <span class='interval'>minute{1} ago</span>", ts.Minutes, ts.Minutes == 1 ? "" : "s");
            }
            else
            {
                result = (ts.TotalDays < 1)
                    ? string.Format("<span class='number'>{0}</span> <span class='interval'>hour{1} ago</span>", ts.Hours, ts.Hours == 1 ? "" : "s")
                    : string.Format("<span class='interval'>{0}</span>", utcDateTime.ToLocalTime().ToString("dd MMM yyyy HH:mm"));
            }

            return new MvcHtmlString(result);
        }

        public static string ToStringLocal(this DateTime? utcNullableDateTime, string format = null)
        {
            return utcNullableDateTime.HasValue ? utcNullableDateTime.Value.ToLocalTime().ToString(format) : null;
        }

        public enum UnitOfTime
        {
            Year,
            Month,
            Day,
            Hour,
            Minute,
            Second
        }

        public static string ToWordy(this TimeSpan timespan, UnitOfTime smallestUnit)
        {
            var sb = new StringBuilder();
            var components = timespan.ToComponents(smallestUnit).ToList();

            for (int ii = 0; ii < components.Count; ii++)
            {
                var component = components[ii];

                if (ii != 0)
                {
                    sb.Append(ii == components.Count - 1 ? " & " : ", ");
                }

                sb.AppendFormat("{0} {1}{2}", component.Item1, component.Item2.ToString().ToLower(), component.Item1 == 1 ? "" : "s");
            }

            return sb.ToString();
        }

        public static IEnumerable<Tuple<int, UnitOfTime>> ToComponents(this TimeSpan timespan, UnitOfTime smallestUnit)
        {
            var years = timespan.TotalDays / 365;

            if (years >= 1)
                yield return Tuple.Create((int) years, UnitOfTime.Year);

            if (smallestUnit == UnitOfTime.Year)
                yield break;

            var months = timespan.TotalDays/30 - (years * 12);

            if (months >= 1)
                yield return Tuple.Create((int) months, UnitOfTime.Month);

            if (smallestUnit == UnitOfTime.Month)
                yield break;

            var days = timespan.TotalDays%30;

            if (days >= 1)
                yield return Tuple.Create((int) days, UnitOfTime.Day);

            if (smallestUnit == UnitOfTime.Day)
                yield break;

            if (timespan.Hours >= 1)
                yield return Tuple.Create(timespan.Hours, UnitOfTime.Hour);

            if (smallestUnit == UnitOfTime.Hour)
                yield break;

            if (timespan.Minutes >= 1)
                yield return Tuple.Create(timespan.Minutes, UnitOfTime.Minute);

            if (smallestUnit == UnitOfTime.Minute)
                yield break;
            ;
            if (timespan.Seconds >= 1)
                yield return Tuple.Create(timespan.Seconds, UnitOfTime.Second);

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
