using System.Xml.Linq;

namespace Errordite.Core.Extensions
{
    public static class XElementExtensions
    {
        public static string SafeAttributeValue(this XElement element, string attributeName)
        {
            var selectedAttribute = element.Attribute(attributeName);
            return selectedAttribute == null ? null : selectedAttribute.Value;
        }
    }
}