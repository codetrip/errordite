
using System.Collections;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Errordite.Core.Auditing.Entities;

namespace Errordite.Core.Auditing
{
    /// <summary>
    /// Class to assist with ascertaining debug information from exception and the stacktrace
    /// </summary>
    public static class ExceptionUtility
    {
        #region Public Methods

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static String FormatException(Exception exception)
        {
            return FormatException(exception, false, false);
        }

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="appendEnvironmentInfo">if set to <c>true</c> [append environment info].</param>
        /// <returns></returns>
        public static String FormatException(Exception exception, bool appendEnvironmentInfo)
        {
            return FormatException(exception, appendEnvironmentInfo, false);
        }


        /// <summary>
        /// Gets the exception message but does not work to make this suitable for output
        /// via an auditor (braces are not replaced which, if the exception message contains
        /// them will cause an exception in the auditor as it treats these as string format
        /// parameter placeholders)
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="appendEnvironmentInfo"></param>
        /// <param name="reflectException"></param>
        /// <returns></returns>
        public static String FormatException(Exception exception, bool appendEnvironmentInfo, bool reflectException)
        {
            return FormatException(exception, appendEnvironmentInfo, reflectException, false);
        }

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="appendEnvironmentInfo">if set to <c>true</c> [append environment info].</param>
        /// <param name="reflectException">if set to <c>true</c> [reflect exception].</param>
        /// <param name="isAuditOutput">This should be set to true if the output of this
        /// is going to be sent to an <see cref="IComponentAuditor"/>. It ensures that
        /// any braces are escaped otherwise they will cause an exception when 
        /// the auditor executes its format string parameter replacements</param>
        /// <returns></returns>
        /// <returns></returns>
        public static String FormatException(Exception exception,
            bool appendEnvironmentInfo,
            bool reflectException,
            bool isAuditOutput)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            var eventInformation = new StringBuilder();

            eventInformation.AppendLine();
            eventInformation.AppendLine("================================================================================================");
            eventInformation.AppendLine("Exception Info [Start]");
            eventInformation.AppendLine();

            //append system info if requested
            if (appendEnvironmentInfo)
                AppendEnvironmentInfo(eventInformation);

            var currException = exception;
            var exceptionCount = 0;

            do
            {
                if (exceptionCount > 0)
                {
                    eventInformation.AppendLine();
                    eventInformation.AppendLine("Exception Info [Start Inner Exception]");
                    eventInformation.AppendLine("================================================================================================");
                }

                eventInformation.AppendLine(String.Format("Type             : {0}", currException.GetType().FullName));
                eventInformation.AppendLine(String.Format("Message          : {0}", currException.Message));

                if (currException.Data != null && currException.Data.Count > 0)
                {
                    foreach (DictionaryEntry dataItem in currException.Data)
                    {
                        eventInformation.AppendLine(String.Format("{0}             : {1}", dataItem.Key, dataItem.Value));
                    }
                }

                if (reflectException)
                    ReflectException(currException, eventInformation);

                // Record the StackTrace with separate label.
                if (currException.StackTrace != null)
                {
                    eventInformation.AppendLine("StackTrace:");
                    eventInformation.AppendLine(currException.StackTrace);
                }
                else
                    eventInformation.AppendLine();

                // Reset the temp exception object and iterate the counter.
                currException = currException.InnerException;
                exceptionCount++;

            } while (currException != null);

            eventInformation.AppendLine("Exception Info [End]");
            eventInformation.Append("================================================================================================");

            if (isAuditOutput)
            {
                eventInformation = eventInformation.Replace("{", "{{");
                eventInformation = eventInformation.Replace("}", "}}");
            }

            return eventInformation.ToString();
        }

        /// <summary>
        /// Returns a text representation of the stack trace with source information if available.
        /// </summary>
        /// <param name="stackTrace">The source to represent textually.</param>
        /// <returns>The textual representation of the stack.</returns>
        public static String GetStackTraceWithSourceInfo(StackTrace stackTrace)
        {
            var buffer = new StringBuilder(255);

            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var stackFrame = stackTrace.GetFrame(i);

                buffer.Append(String.Format("{0}at", Environment.NewLine));

                var method = stackFrame.GetMethod();
                var t = method.DeclaringType;

                if (t != null)
                {
                    var nameSpace = t.Namespace;
                    if (nameSpace != null)
                    {
                        buffer.Append(nameSpace);
                        buffer.Append(".");
                    }

                    buffer.Append(t.Name);
                    buffer.Append(".");
                }
                buffer.Append(method.Name);
                buffer.Append("(");

                var arrParams = method.GetParameters();

                for (var j = 0; j < arrParams.Length; j++)
                {
                    var typeName = "<UnknownType>";
                    if (arrParams[j].ParameterType != null)
                    {
                        typeName = arrParams[j].ParameterType.Name;
                    }

                    buffer.Append((j != 0 ? ", " : "") + typeName + " " + arrParams[j].Name);
                }

                buffer.Append(")");

                if (stackFrame.GetILOffset() != -1)
                {
                    // It's possible we have a debug version of an executable but no PDB.  In
                    // this case, the file name will be null.
                    var fileName = stackFrame.GetFileName();

                    if (fileName != null)
                    {
                        buffer.Append(
                            String.Format(
                                Thread.CurrentThread.CurrentCulture,
                                "{0}in {1}: line {2}",
                                Environment.NewLine,
                                fileName,
                                stackFrame.GetFileLineNumber()));
                    }
                }
            }

            return buffer.ToString();
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Reflects the exception.
        /// </summary>
        /// <param name="currException">The curr exception.</param>
        /// <param name="eventInformation">The event information.</param>
        private static void ReflectException(Exception currException, StringBuilder eventInformation)
        {
            foreach (var propinfo in currException.GetType().GetProperties())
            {
                if (propinfo != null)
                {
                    // Do not log information for the InnerException or StackTrace. 
                    // This information is captured later in the process.
                    if (propinfo.Name != "InnerException" && propinfo.Name != "StackTrace" && propinfo.Name != "Message" && propinfo.Name != "Data")
                    {
                        try
                        {
                            if (propinfo.GetValue(currException, null) == null)
                            {
                                eventInformation.AppendLine(String.Format("{0}: Null", propinfo.Name));
                            }
                            else
                            {
                                ProcessAdditionalInfo(propinfo, currException, eventInformation);
                            }
                        }
                        catch
                        {
                            //if we get an exception when accessing a property just swallow it up
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the additional info.
        /// </summary>
        /// <param name="propinfo">The propinfo.</param>
        /// <param name="currException">The curr exception.</param>
        /// <param name="eventInformation">The event information.</param>
        private static void ProcessAdditionalInfo(PropertyInfo propinfo, Exception currException, StringBuilder eventInformation)
        {
            NameValueCollection currAdditionalInfo;

            // Loop through the collection of AdditionalInformation if the exception type is a BaseApplicationException.
            if (propinfo.Name == "AdditionalInformation")
            {
                if (propinfo.GetValue(currException, null) != null)
                {
                    // Cast the collection Int32o a local variable.
                    currAdditionalInfo = (NameValueCollection)propinfo.GetValue(currException, null);

                    // Check if the collection contains values.
                    if (currAdditionalInfo.Count > 0)
                    {
                        eventInformation.AppendLine("Additional Information:");

                        // Loop through the collection adding the information to the String builder.
                        for (var i = 0; i < currAdditionalInfo.Count; i++)
                        {
                            eventInformation.AppendLine(String.Format("{0}: {1}", currAdditionalInfo.GetKey(i), currAdditionalInfo[i]));
                        }
                    }
                }
            }
            else
            {
                // Otherwise just write the ToString() value of the property.
                eventInformation.AppendLine(String.Format("{0}: {1}", propinfo.Name, propinfo.GetValue(currException, null)));
            }
        }

        /// <devdoc>
        /// Add additional 'environment' information. 
        /// </devdoc>
        private static void AppendEnvironmentInfo(StringBuilder eventInformation)
        {
            eventInformation.AppendLine("Machine Name     : " + Environment.MachineName);
            eventInformation.AppendLine("Timestamp        : " + DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
            eventInformation.AppendLine("AppDomain Name   : " + AppDomain.CurrentDomain.FriendlyName);
            eventInformation.AppendLine("Windows Identity : " + WindowsIdentity.GetCurrent().Name);
        }

        #endregion
    }
}