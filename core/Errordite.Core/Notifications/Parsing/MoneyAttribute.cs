using System;

namespace Errordite.Core.Notifications.Parsing
{
    public class MoneyAttribute : Attribute, IStructConverter<decimal>
    {
        public string Convert(decimal o)
        {
            return o.ToString("0.00");
        }

        public string Convert(decimal? o)
        {
            return o.HasValue ? Convert(o.Value) : string.Empty;
        }
    }
}