using System.Xml.Linq;

namespace CodeTrip.Core.Extensions
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