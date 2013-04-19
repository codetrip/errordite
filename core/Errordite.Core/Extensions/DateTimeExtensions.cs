using System;

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
    }
}
