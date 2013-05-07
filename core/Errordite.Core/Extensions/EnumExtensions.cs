using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace Errordite.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

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
