using System;
using Errordite.Core.Identity;

namespace Errordite.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToLocalTimeFormatted(this DateTime datetimeUtc)
        {
            return datetimeUtc.ToLocal().ToString("dd MMM yyyy HH:mm:ss");
        }

        public static string ToLocalFormatted(this DateTime datetimeUtc)
        {
            return datetimeUtc.ToLocal().ToString("dd MMM yyyy");
        }

        public static DateTime ToLocal(this DateTime datetimeUtc)
        {
            var appContext = AppContext.GetFromHttpContext();
            string timezoneId;
            if (appContext != null && (timezoneId = appContext.CurrentUser.EffectiveTimezoneId()) != null)
            {
                return 
                    TimeZoneInfo.ConvertTimeBySystemTimeZoneId(datetimeUtc, timezoneId);
            }
            else
            {
                return datetimeUtc.ToLocalTime();
            }
        }
    }
}
