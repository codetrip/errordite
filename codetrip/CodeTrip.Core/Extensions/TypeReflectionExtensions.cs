using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeTrip.Core.Extensions
{
    public static class TypeReflectionExtensions
    {
        public static IEnumerable<string> StringConstantValues(this Type type)
        {
            return from field in type.GetFields(BindingFlags.Static | BindingFlags.Public)
                   where field.IsLiteral && field.FieldType == typeof(string)
                   select (string)field.GetValue(null);
        }

        public static object PropertyValue<T>(this T instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName).GetValue(instance, null);
        }
    }
}
