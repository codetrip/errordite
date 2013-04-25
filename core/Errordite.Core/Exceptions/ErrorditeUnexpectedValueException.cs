
using Errordite.Core.Extensions;

namespace Errordite.Core.Exceptions
{
    public class ErrorditeUnexpectedValueException : ErrorditeException
    {
        public ErrorditeUnexpectedValueException(string whatHadTheUnexpectedValue, string whatWasTheUnexpectedValue)
            :base("Cannot handle a {0} value of {1}".FormatWith(whatWasTheUnexpectedValue, whatHadTheUnexpectedValue))
        {
        }
    }
}