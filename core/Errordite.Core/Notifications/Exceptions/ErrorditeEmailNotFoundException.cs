using Errordite.Core.Exceptions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Notifications.Exceptions
{
    public class ErrorditeEmailNotFoundException : ErrorditeException
    {
        public ErrorditeEmailNotFoundException(string templateLocation)
            :base("Template not found at {0}".FormatWith(templateLocation))
        {}
    }
}