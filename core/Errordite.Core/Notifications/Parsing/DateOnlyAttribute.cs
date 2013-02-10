using System;

namespace Errordite.Core.Notifications.Parsing
{
    public class DateOnlyAttribute : Attribute, IStructConverter<DateTime>
    {
        public string Convert(DateTime dt)
        {
            return dt.ToShortDateString();
        }

        public string Convert(DateTime? dt)
        {
            return dt.HasValue ? Convert(dt.Value) : "";
        }
    }
}