
using System;
using Errordite.Client;
using NUnit.Framework;

namespace Errordite.Test
{
    [TestFixture]
    public class GenerateErrors
    {
        [Test]
        public void GenerateError()
        {
            try
            {
                int t = 0;
                int res = 100/t;
                Console.Write(res);
            }
            catch (Exception e)
            {
                ErrorditeClient.ReportException(e, false);
            }
        }

        [Test]
        public void LocalTime()
        {
            var date = DateTime.UtcNow;
            var timezone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            var local = TimeZoneInfo.ConvertTimeFromUtc(date, timezone);
            var time = timezone.GetUtcOffset(date);
            Console.WriteLine(date.ToLongTimeString());
            Console.WriteLine(date.ToUniversalTime().ToLongTimeString());
            Console.WriteLine(date.ToLocalTime().ToLongTimeString());
            var offset = new DateTimeOffset(local, time);
            Console.Write(offset.ToString());
        }
    }
}
