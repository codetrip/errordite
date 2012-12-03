using System;

namespace Errordite.Core.Notifications.Parsing
{
    public class TimeOnlyAttribute : Attribute, IStructConverter<DateTime>
    {
        public string Convert(DateTime dt)
        {
            return dt.ToShortTimeString();
        }

        public string Convert(DateTime? dt)
        {
            return dt.HasValue ? Convert(dt.Value) : "";
        }
    }
}
