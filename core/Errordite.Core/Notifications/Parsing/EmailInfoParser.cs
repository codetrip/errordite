using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using CodeTrip.Core.Extensions;
using Errordite.Core.Configuration;
using Errordite.Core.Notifications.EmailInfo;

namespace Errordite.Core.Notifications.Parsing
{
    public class EmailInfoParser : IEmailInfoParser
    {
        private readonly EmailConfiguration _config;

        public EmailInfoParser(EmailConfiguration config)
        {
            _config = config;
        }

        public IDictionary<string, string> Parse(EmailInfoBase emailInfo)
        {
            var ret = emailInfo.GetReadableProperties().SelectMany(p => GetParameters(p, emailInfo));

            ret = ret.Concat(new[]
            {
                new KeyValuePair<string, string>("ErrorditeUrl", _config.ErrorditeUrl),
            });
            
            return ret.ToDictionary(x => x.Key, x => x.Value);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetParameters(PropertyInfo propertyInfo, object o)
        {
            object oValue = propertyInfo.GetValue(o, null);

            var converter = GetAttributeImplementingInterface<IClassConverter>(propertyInfo);
            if (converter != null)
            {
                //converter is responsible for doing HttpEncode
                yield return new KeyValuePair<string, string> (propertyInfo.Name, converter.Convert(oValue));
                yield break;
            }

            //if it's a DateTime or decimal, it might have a converter attribute associated with it
            string ret;
            if (Convert<DateTime>(propertyInfo, o, out ret) || Convert<decimal>(propertyInfo, o, out ret))
            {
                yield return new KeyValuePair<string, string>(propertyInfo.Name, ret);
                yield break;
            }

            foreach (var param in GetParameters(propertyInfo.Name, oValue))
                yield return param;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetParameters(string propertyName, object oValue)
        {
            //if it's a string, just add the value to the dictionary.  Do this first or it will also pass the "IEnumerable" test.
            if (oValue is string)
            {
                yield return new KeyValuePair<string, string>(propertyName, HttpUtility.HtmlEncode((string)oValue));
                yield break;
            }

            //no conversion of IEnumerable of complex types supported yet so just convert straight to string
            if (oValue is IEnumerable)
            {
                int ii = 0;
                foreach (var eValue in (IEnumerable)oValue)
                {
                    ii++;
                    foreach (var param in GetParameters(propertyName, eValue))
                        yield return new KeyValuePair<string, string>(param.Key + "::" + ii, param.Value);
                }

                yield break;
            }

            //assume that any Errordite type is a complex type
            if (oValue != null && oValue.GetType().FullName.StartsWith("Errordite", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var prop in oValue.GetReadableProperties())
                {
                    foreach(var param in GetParameters(prop, oValue))
                        yield return new KeyValuePair<string, string> ("{0}.{1}".FormatWith(propertyName, param.Key), param.Value);
                }
                yield break;
            }

            //anything else just convert to a string
            yield return new KeyValuePair<string, string>(propertyName, HttpUtility.HtmlEncode((oValue ?? "").ToString()));
        }

        private static bool Convert<TProperty>(PropertyInfo propertyInfo, object emailInfo, out string sValue) where TProperty : struct 
        {
            var t = propertyInfo.PropertyType;

            if (t == typeof(TProperty) || t == typeof(TProperty?))
            {
                var c = GetAttributeImplementingInterface<IStructConverter<TProperty>>(propertyInfo);
                if (c != null)
                {
                    sValue = t == typeof(TProperty) ? c.Convert((TProperty) propertyInfo.GetValue(emailInfo, null)) : c.Convert((TProperty?) propertyInfo.GetValue(emailInfo, null));
                    return true;
                }
            }

            sValue = null;
            return false;
        }

        private static TInterface GetAttributeImplementingInterface<TInterface>(MemberInfo member)
        {
            return (TInterface)member.GetCustomAttributes(false).Where(a => a is TInterface).FirstOrDefault();
        }
    }
}