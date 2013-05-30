using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Errordite.Core.Extensions;

namespace Errordite.Core.Serialisation
{
    public class PropertyIsXmlAttribute : Attribute
    { }

    public static class SimpleXmlSerializer
    {
        public static XElement ToXml(object obj)
        {
            Type objectType = obj.GetType();
            XElement objectGraph = new XElement(CleanName(objectType.Name));
            SerializeProperties(objectType.GetProperties(), obj, objectGraph);
            return objectGraph;
        }

        /// <summary>
        /// Iterate over the objects properties rendering an XElement type for each valid property
        /// </summary>
        /// <param name="propertyInfo">The list of PropertyInfo associated with the object.</param>
        /// <param name="obj">The object we are operating on.</param>
        /// <param name="objectGraph">The root object graph.</param>
        private static void SerializeProperties(IEnumerable<PropertyInfo> propertyInfo, object obj, XContainer objectGraph)
        {
            foreach (PropertyInfo property in propertyInfo)
            {
                try
                {
                    if (!property.PropertyType.IsVisible || !property.PropertyType.IsPublic || ShouldIgnore(property))
                        continue;

                    object propertyValue = property.GetValue(obj, null);

                    if (propertyValue == null)
                        continue;

                    if (PropertyIsXml(property))
                    {
                        if (!propertyValue.ToString().IsNotNullOrEmpty())
                            objectGraph.Add(new XElement(CleanName(property.Name), XElement.Parse(propertyValue.ToString())));
                    }
                    else if (IsSimpleType(property.PropertyType))
                    {
                        objectGraph.Add(new XElement(CleanName(property.Name), propertyValue));
                    }
                    else if (propertyValue is IEnumerable)
                    {
                        SerializeIEnumerable(objectGraph, property, propertyValue);
                    }
                    else
                    {
                        SerializeComplexType(objectGraph, property, propertyValue);
                    }
                }
                catch (Exception e)
                {
                    objectGraph.Add(new XElement(CleanName(property.Name), "ERROR:=" + e.Message));
                }
            }
        }

        private static string CleanName(IEnumerable<char> name)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in name)
            {
                if ((c >= 48 && c <= 57) || ((c >= 65 && c <= 90)) || ((c >= 97 && c <= 122)))
                    result.Append(c.ToString());
            }

            return result.ToString();
        }

        /// <summary>
        /// Determines whether the type is simple, if it is a simple type, we just call ToString() to get the value
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private static bool ShouldIgnore(ICustomAttributeProvider property)
        {
            return property.GetCustomAttributes(typeof(XmlIgnoreAttribute), false).Length > 0;
        }

        /// <summary>
        /// Determines whether the type is simple, if it is a simple type, we just call ToString() to get the value
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private static bool PropertyIsXml(ICustomAttributeProvider property)
        {
            return property.GetCustomAttributes(typeof(PropertyIsXmlAttribute), false).Length > 0;
        }

        /// <summary>
        /// Determines whether the type is simple, if it is a simple type, we just call ToString() to get the value
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <returns></returns>
        private static bool IsSimpleType(Type typeToCheck)
        {
            return typeToCheck.IsValueType || typeToCheck.IsPrimitive || typeToCheck.Equals(typeof(string)) || typeToCheck.IsEnum;
        }

        /// <summary>
        /// Uses recursion to serialize a complex type
        /// </summary>
        /// <param name="objectGraph">The root object graph.</param>
        /// <param name="property">The current property</param>
        /// <param name="propertyValue">The current properties value</param>
        private static void SerializeComplexType(XContainer objectGraph, PropertyInfo property, object propertyValue)
        {
            //create new child element
            XElement nestedType = new XElement(CleanName(property.Name));

            //serialize the child elements properties
            SerializeProperties(property.PropertyType.GetProperties(), propertyValue, nestedType);

            //add it to the objectGraph if the child has at least one child
            if (nestedType.HasElements)
                objectGraph.Add(nestedType);
        }

        /// <summary>
        /// Serializes any type which implements IEnumerable
        /// </summary>
        /// <param name="objectGraph">The root object graph.</param>
        /// <param name="property">The current property</param>
        /// <param name="propertyValue">The current properties value</param>
        private static void SerializeIEnumerable(XContainer objectGraph, PropertyInfo property, object propertyValue)
        {
            XElement nestedType = new XElement(property.Name);

            foreach (object enumerableProperty in (IEnumerable)propertyValue)
            {
                Type enumerableType = enumerableProperty.GetType();
                string elementName = enumerableType.IsGenericType ? "Item" : enumerableType.Name;

                if (IsSimpleType(enumerableType))
                {
                    nestedType.Add(new XElement(CleanName(elementName), enumerableProperty.ToString()));
                }
                else
                {
                    XElement enumerableNestedType = new XElement(CleanName(elementName));
                    nestedType.Add(enumerableNestedType);
                    SerializeProperties(enumerableType.GetProperties(), enumerableProperty, enumerableNestedType);
                }
            }

            if (nestedType.HasElements)
                objectGraph.Add(nestedType);
        }
    }
}
