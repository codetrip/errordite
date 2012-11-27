
using CodeTrip.Core.Exceptions;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Notifications.Exceptions
{
    public class ErrorditeEmailParameterNotFoundException : CodeTripException
    {
        public ErrorditeEmailParameterNotFoundException(string param)
            : base("Parameter {0} in email template was not in corresponding EmailInfo class.".FormatWith(param))
        {}
    }
}
