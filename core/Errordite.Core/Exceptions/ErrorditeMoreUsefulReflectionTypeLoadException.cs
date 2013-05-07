using System;
using System.Reflection;
using Errordite.Core.Extensions;

namespace Errordite.Core.Exceptions
{
    public class ErrorditeMoreUsefulReflectionTypeLoadException : ErrorditeException
    {
        public ErrorditeMoreUsefulReflectionTypeLoadException(ReflectionTypeLoadException reflectionTypeLoadException)
            :base("ReflectionTypeLoadException.  Message: {0}.  LoaderExceptions: {1}"
                      .FormatWith(reflectionTypeLoadException, reflectionTypeLoadException.LoaderExceptions.StringConcat(
                          lex => "LOADER EXCEPTION: " + lex.Message + Environment.NewLine)),
                  false,
                  reflectionTypeLoadException)
        {
            
        }
    }
}