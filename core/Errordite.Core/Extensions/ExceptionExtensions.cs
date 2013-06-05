using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Errordite.Core.Extensions;

namespace Errordite.Core.Extensions
{
    public static class ExceptionExtensions
    {
        private static readonly Regex _regexStackTrace = new Regex(@"
                ^
                \s*
                \w+ \s+ 
                (?<type> .+ ) \.
                (?<method> .+? ) 
                (?<params> \( (?<params> .*? ) \) )
                ( \s+ 
                \w+ \s+ 
                  (?<file> [a-z] \: .+? ) 
                  \: \w+ \s+ 
                  (?<line> [0-9]+ ) \p{P}? )?
                \s*
                $",
               RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


        public static Exception SetCustomData(this Exception e, string key, string value)
        {
            e.Data.Add(key, value);
            return e;
        }

        public static string MarkUpStackTrace(this string stackTrace)
        {
            return stackTrace.IsNullOrEmpty() ? string.Empty : DoMarkupStackTrace(stackTrace);
        }

        private static string DoMarkupStackTrace(string text)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            Debug.Assert(text != null);
            Debug.Assert(writer != null);

            int anchor = 0;

            foreach (Match match in _regexStackTrace.Matches(text))
            {
                HttpUtility.HtmlEncode(text.Substring(anchor, match.Index - anchor), writer);
                MarkupStackFrame(text, match, writer);
                anchor = match.Index + match.Length;
            }

            HttpUtility.HtmlEncode(text.Substring(anchor), writer);

            return sb.ToString();
        }

        private static void MarkupStackFrame(string text, Match match, TextWriter writer)
        {
            int anchor = match.Index;
            GroupCollection groups = match.Groups;

            //
            // Type + Method
            //

            Group type = groups["type"];
            HttpUtility.HtmlEncode(text.Substring(anchor, type.Index - anchor), writer);
            anchor = type.Index;
            writer.Write("<span class='st-frame'>");
            anchor = StackFrameSpan(text, anchor, "st-type", type, writer);
            anchor = StackFrameSpan(text, anchor, "st-method", groups["method"], writer);

            //
            // Parameters
            //

            Group parameters = groups["params"];
            HttpUtility.HtmlEncode(text.Substring(anchor, parameters.Index - anchor), writer);
            writer.Write("<span class='st-params'>(");
            int position = 0;
            foreach (string parameter in parameters.Captures[0].Value.Split(','))
            {
                int spaceIndex = parameter.LastIndexOf(' ');
                if (spaceIndex <= 0)
                {
                    Span(writer, "st-param", parameter.Trim());
                }
                else
                {
                    if (position++ > 0)
                        writer.Write(", ");
                    string argType = parameter.Substring(0, spaceIndex).Trim();
                    Span(writer, "st-param-type", argType);
                    writer.Write(' ');
                    string argName = parameter.Substring(spaceIndex + 1).Trim();
                    Span(writer, "st-param-name", argName);
                }
            }
            writer.Write(")</span>");
            anchor = parameters.Index + parameters.Length;

            //
            // File + Line
            //

            anchor = StackFrameSpan(text, anchor, "st-file", groups["file"], writer);
            anchor = StackFrameSpan(text, anchor, "st-line", groups["line"], writer);

            writer.Write("</span>");

            //
            // Epilogue
            //

            int end = match.Index + match.Length;
            HttpUtility.HtmlEncode(text.Substring(anchor, end - anchor), writer);
        }

        private static int StackFrameSpan(string text, int anchor, string klass, Group group, TextWriter writer)
        {
            return group.Success
                 ? StackFrameSpan(text, anchor, klass, group.Value, group.Index, group.Length, writer)
                 : anchor;
        }

        private static int StackFrameSpan(string text, int anchor, string klass, string value, int index, int length, TextWriter writer)
        {
            HttpUtility.HtmlEncode(text.Substring(anchor, index - anchor), writer);
            Span(writer, klass, value);
            return index + length;
        }

        private static void Span(TextWriter writer, string klass, string value)
        {
            writer.Write("<span class='");
            writer.Write(klass);
            writer.Write("'>");
            HttpUtility.HtmlEncode(value, writer);
            writer.Write("</span>");
        }
    }
}
