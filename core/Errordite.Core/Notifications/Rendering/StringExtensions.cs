using System;

namespace Errordite.Core.Notifications.Rendering
{
    internal static class StringExtensions
    {
        public static string Prefix(this string s)
        {
            return s.Split(':')[0];
        }

        public static string Suffix(this string s)
        {
            string[] parts;
            return (parts = s.Split(':')).Length > 1 ? parts[1] : null;
        }

        public static string Prefix(this string s, string separator)
        {
            return s.Split(new []{separator}, StringSplitOptions.None)[0];
        }

        public static string Suffix(this string s, string separator)
        {
            string[] parts;
            return (parts = s.Split(new[] { separator }, StringSplitOptions.None)).Length > 1 ? parts[1] : null;
        }
    }
}