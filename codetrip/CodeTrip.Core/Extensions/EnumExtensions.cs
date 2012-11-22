using System;
using System.Resources;

namespace CodeTrip.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string MapToResource(this Enum enumValue, ResourceManager resource)
        {
            string resourceKey = enumValue.GetType().Name + "_" + enumValue;
            return resource.GetString(resourceKey);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FriendlyNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public FriendlyNameAttribute(string name)
        {
            Name = name;
        }
    }
}
