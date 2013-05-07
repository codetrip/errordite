
using Errordite.Core.Exceptions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Notifications.Exceptions
{
    public class ErrorditeEmailParameterNotFoundException : ErrorditeException
    {
        public ErrorditeEmailParameterNotFoundException(string param)
            : base("Parameter {0} in email template was not in corresponding EmailInfo class.".FormatWith(param))
        {}
    }
}
