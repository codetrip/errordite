using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Errordite.Core.Extensions;

namespace Errordite.Core
{
    public class Duration
    {
        private int _months;
        private int _weeks;
        private int _days;
        private int _hours;

        private static readonly Regex _regex = new Regex(@"((?'Months'\d+)M)?((?'Weeks'\d+)w)?((?'Days'\d+)d)?((?'Hours'\d+)h)?", RegexOptions.Compiled);

        public Duration(string encoded)
        {
            if (encoded == null || encoded == "0")
                return;

            var m = _regex.Match(encoded);

            if (!m.Success)
                throw new ErrorditeInvalidDurationStringException(encoded);

            _months = GetValue(m, "Months");
            _weeks = GetValue(m, "Weeks");
            _days = GetValue(m, "Days");
            _hours = GetValue(m, "Hours");
        }

        private int GetValue(Match match, string groupName)
        {
            var g = match.Groups[groupName];
            if (g.Success)
                return int.Parse(g.Value);
            return 0;
        }

        public Duration(int months = 0, int weeks = 0, int days = 0, int hours = 0)
        {
            _months = months;
            _weeks = weeks;
            _days = days;
            _hours = hours;
        }

        public override string ToString()
        {
            return "{0}M{1}w{2}d{3}h".FormatWith(_months, _weeks, _days, _hours);
        }

        public string Description
        {
            get
            {
                var parts = GetDescriptionParts();

                if (!parts.Any())
                    return null;

                return parts.StringConcat(", ", trimEnd: true, lastDelimiter: " & ");
            }
        }

        private IEnumerable<string> GetDescriptionParts()
        {
            if (_months > 0)
                yield return "month".Quantity(_months);

            if (_weeks > 0)
                yield return "week".Quantity(_weeks);

            if (_days > 0)
                yield return "day".Quantity(_days);

            if (_hours > 0)
                yield return "hour".Quantity(_hours);

        }

        public  static DateTime operator -(DateTime dt, Duration duration)
        {
            return dt.AddMonths(-duration._months)
                     .AddDays(-(duration._weeks*7 + duration._days))
                     .AddHours(-duration._hours);
        }

        public static DateTime operator +(DateTime dt, Duration duration)
        {
            return dt.AddMonths(duration._months)
                     .AddDays(duration._weeks * 7 + duration._days)
                     .AddHours(duration._hours);
        }
    }

    public class ErrorditeInvalidDurationStringException : Exception
    {
        public ErrorditeInvalidDurationStringException(string encoded)
            :base("Invalid encoded duration string: " + encoded)
        {
            
        }
    }
}