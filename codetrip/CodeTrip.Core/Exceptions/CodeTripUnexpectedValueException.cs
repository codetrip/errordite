
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Exceptions
{
    public class CodeTripUnexpectedValueException : CodeTripException
    {
        public CodeTripUnexpectedValueException(string whatHadTheUnexpectedValue, string whatWasTheUnexpectedValue)
            :base("Cannot handle a {0} value of {1}".FormatWith(whatWasTheUnexpectedValue, whatHadTheUnexpectedValue))
        {
        }
    }
}