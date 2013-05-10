using System;
using System.Collections.Specialized;
using System.Web;

namespace Errordite.Core.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static NameValueCollection MergeWith(this NameValueCollection source, NameValueCollection target)
        {
            foreach (var key in target.AllKeys)
            {
                if ((source.Get(key)) == null)
                    source.Add(key, target.Get(key));
                else
                    source.Set(key, target.Get(key));
            }

            return source;
        }

        public static string ToQueryString(this NameValueCollection source)
        {
            return string.Join("&", Array.ConvertAll(source.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(source[key]))));
        }
    }
}
