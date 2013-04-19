using System;
using Errordite.Core.Extensions;
using Errordite.Core.Domain;

namespace Errordite.Core.Notifications.Parsing
{
    public class FriendlyIdAttribute : Attribute, IClassConverter
    {
        public string Convert(object o)
        {
            if (o == null)
                return null;

            var s = o as string;
            if (s == null)
                return o.ToString();

            return IdHelper.GetFriendlyId(s);
        }
    }
}