using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeTrip.Core.Extensions
{
    public static class ObjectReflectionExtensions
    {
        public static IEnumerable<PropertyInfo> GetReadableProperties(this object o)
        {
            return o.GetType().GetTypeReadableProperties();
        }

        public static IEnumerable<PropertyInfo> GetWritableProperties(this object o)
        {
            return
                o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Where(
                    p => p.CanWrite);
        }

        public static IEnumerable<PropertyInfo> GetReadableWritableProperties(this object o)
        {
            return
                o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Where(
                    p => p.CanWrite && p.CanRead);
        }

        public static IEnumerable<PropertyInfo> GetTypeWritableProperties(this Type t)
        {
            return
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Where(
                    p => p.CanWrite);
        }

        public static IEnumerable<PropertyInfo> GetTypeReadableProperties(this Type t)
        {
            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Where(p => p.CanRead);
        }
    }
}
