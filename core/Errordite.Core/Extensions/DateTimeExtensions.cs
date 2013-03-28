using System;

namespace Errordite.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToLocalTimeFormatted(this DateTimeOffset datetimeUtc)
        {
            return datetimeUtc.ToString("dd MMM yyyy HH:mm:ss");
        }

        public static string ToLocalFormatted(this DateTimeOffset datetimeUtc)
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
