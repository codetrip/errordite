using System;

namespace CodeTrip.Core.Extensions
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
    }
}
