using System;

namespace Errordite.Core.Extensions
{
    public static class DoubleExtensions
    {
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
		{
			var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return origin.AddSeconds(timestamp);
		}
    }
}
