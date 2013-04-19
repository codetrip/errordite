using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Errordite.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex StripCssRegex = new Regex(@"(.*)(\<style\>.*\<\/style\>)(.*)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex StripCommentsRegex = new Regex(@"(.*)(\<!.*-\>)(.*)", RegexOptions.Compiled | RegexOptions.Multiline);

        public static int SafeParseInt(this string s)
        {
            int result;
            if (!int.TryParse(s, out result))
                return -1;

            return result;
        }

        public static string StripCss(this string input)
        {
            if (input.IsNullOrEmpty())
                return null;

            var result = StripCssRegex.Replace(input, "$1$3");
            return StripCommentsRegex.Replace(result, "$1$3");
        }

        public static Dictionary<string, T> ToDictionary<T>(this string s, Func<string, T> valueAction)
        {
            if (s.IsNullOrEmpty())
                return new Dictionary<string, T>();

            return s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(entry => entry.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                    .ToDictionary(kvp => kvp[0], kvp => valueAction(kvp.Length > 1 ? kvp[1] : null));
        }

        public static string GetFriendlyId(this string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s.Contains("/") ? s.Split('/')[1] : s;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNotNullOrEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        public static string RavenEscape(this string s)
        {
            return s.Replace(":", "");
        }

        public static string FormatWith(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static MvcHtmlString DoNotEncode(this object o)
        {
            if (o == null)
                return null;

            return new MvcHtmlString(o.ToString());
        }

        public static string LastElement(this string input, char seperator)
        {
            if (input == null)
                return string.Empty;

            string[] elements = input.Split(new[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
            return elements[elements.Length - 1];
        }

        public static string Base64Encode(this string input)
        {
            if (input.IsNullOrEmpty())
                return string.Empty;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public static string Base64Decode(this string input)
        {
            if (input.IsNullOrEmpty())
                return string.Empty;

            return Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }

        public static string ReplaceFirstOccurrance(this string original, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;

            int loc = original.IndexOf(oldValue, StringComparison.Ordinal);
            return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }

        public static string Hash(this string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
        public static string MergeQueryStrings(this string query1, string query2)
        {
            var nvc1 = HttpUtility.ParseQueryString(query1);
            var nvc2 = HttpUtility.ParseQueryString(query2);
            return nvc1.MergeWith(nvc2).ToQueryString();
        }
    }
}
