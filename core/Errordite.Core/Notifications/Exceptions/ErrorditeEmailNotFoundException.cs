using CodeTrip.Core.Exceptions;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Notifications.Exceptions
{
    public class ErrorditeEmailNotFoundException : CodeTripException
    {
        public ErrorditeEmailNotFoundException(string templateLocation)
            :base("Template not found at {0}".FormatWith(templateLocation))
        {}
    }
}