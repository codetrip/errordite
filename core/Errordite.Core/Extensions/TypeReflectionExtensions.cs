using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Errordite.Core.Extensions
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

        public static void SetPrimitiveToString<T>(this T component, string propertyName, string stringValue)
        {
            var property = component.GetType().GetProperty(propertyName);

            if (TrySetProperty(property, component, stringValue, int.Parse))
                return;
            if (TrySetProperty(property, component, stringValue, long.Parse))
                return;
            if (TrySetProperty(property, component, stringValue, decimal.Parse))
                return;
            if (TrySetProperty(property, component, stringValue, DateTime.Parse))
                return;
            if (TrySetProperty(property, component, stringValue, bool.Parse))
                return;
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(component, stringValue, null);
                return;
            }
        }

        private static bool TrySetProperty<T>(PropertyInfo property, object component, string overrideValue, Func<string, T> map)
            where T : struct
        {
            if (!property.PropertyType.IsIn(typeof(T), typeof(T?)))
                return false;

            if (overrideValue.IsNullOrEmpty() && property.PropertyType == typeof(T?))
                property.SetValue(component, null, null);
            else
                property.SetValue(component, map(overrideValue), null);

            return true;
        }
    }
}
