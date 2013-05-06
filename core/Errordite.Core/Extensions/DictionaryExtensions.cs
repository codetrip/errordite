

using System;
using System.Collections.Generic;
using System.Text;

namespace Errordite.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static string GetValue(this Dictionary<string, string> dict, string key)
        {
            if (dict == null)
                return string.Empty;

            return dict.ContainsKey(key) ? dict[key] : string.Empty;
        }

        public static string Serialize<T>(this Dictionary<string, T> dict, Func<T, string> valueSplitter)
        {
            if (dict == null)
                return string.Empty;

            StringBuilder result = new StringBuilder();

            foreach(var kvp in dict)
            {
                result.Append("{0}:{1};".FormatWith(kvp.Key, valueSplitter(kvp.Value)));
            }

            return result.ToString();
        }
    }
}
