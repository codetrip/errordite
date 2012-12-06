using CodeTrip.Core.Exceptions;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Notifications.Exceptions
{
    public class ErrorditeTemplateNotFoundException : CodeTripException
    {
        public ErrorditeTemplateNotFoundException(string templateLocation)
            :base("Template not found at {0}".FormatWith(templateLocation))
        {}
    }
}