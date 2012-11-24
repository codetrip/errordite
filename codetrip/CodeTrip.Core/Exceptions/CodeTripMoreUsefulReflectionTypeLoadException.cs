using System;
using System.Reflection;
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Exceptions
{
    public class CodeTripMoreUsefulReflectionTypeLoadException : CodeTripException
    {
        public CodeTripMoreUsefulReflectionTypeLoadException(ReflectionTypeLoadException reflectionTypeLoadException)
            :base("ReflectionTypeLoadException.  Message: {0}.  LoaderExceptions: {1}"
                      .FormatWith(reflectionTypeLoadException, reflectionTypeLoadException.LoaderExceptions.StringConcat(
                          lex => "LOADER EXCEPTION: " + lex.Message + Environment.NewLine)),
                  false,
                  reflectionTypeLoadException)
        {
            
        }
    }
}