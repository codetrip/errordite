using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Errordite.Core.Dynamic
{
    public class SummaryIgnoreAttribute : Attribute { }

    /// <summary>
    /// Base class for the dynamically generated SummaryWriter classes.
    /// </summary>
    public abstract class EntitySummaryWriterBase
    {
        protected static void AppendPair(string key, object value, StringBuilder sb)
        {
            if (value is DateTime? && ((DateTime?)value).HasValue)
                sb.AppendFormat("{0}:={1}", key, ((DateTime?)value).Value.ToString("yyyy-MM-dd hh:mm:ss.fff"));
            else if (value is DateTime)
                sb.AppendFormat("{0}:={1}", key, ((DateTime)value).ToString("yyyy-MM-dd hh:mm:ss.fff"));
            else
                sb.AppendFormat("{0}:={1}", key, value);
        }
    }

    /// <summary>
    /// Delegate for a method that writes a Summary string for an entity of type T.
    /// </summary>
    public delegate string SummaryWriterDelegate<T>(T entity);

    public static class SummaryWriter
    {
        public static string GetSummary<T>(T entity)
        {
            return SummaryWriter<T>.GetSummary(entity);
        }
    }

    /// <summary>
    /// Writes a summary for an entity in an efficient manner (dynamically writes
    /// and compiles a class then assigns a delegate to a member variable for use in the future.
    /// </summary>
    public static class SummaryWriter<T>
    {
        #region constants
        private const string DynamicNamespace = "Errordite.CodeGen";
        private const bool KeepTempFiles = false;

        private static readonly string DynamicClassName = "EntitySummaryWriter_" + Guid.NewGuid().ToString().Replace("-", string.Empty);

        private static readonly string FQDynamicClassName = DynamicNamespace + "." + DynamicClassName;
        private static readonly object _syncLock = new object();


        private static readonly Type _type = typeof(T);
        #endregion

        #region fields

        private static SummaryWriterDelegate<T> _summaryWriterDelegate;
        #endregion

        /// <summary>
        /// Assigns a delegate for creating a summary if it does not already exist
        /// then returns the results of calling the delegate.
        /// </summary>
        public static string GetSummary(T entity)
        {
            if (entity == null)
                return "(null)";

            if (_summaryWriterDelegate == null)
            {
                lock (_syncLock)
                {
                    if (_summaryWriterDelegate == null)
                        _summaryWriterDelegate = BuildSummaryWriter();
                }
            }

            return _summaryWriterDelegate(entity);
        }

        /// <summary>
        /// Creates a SummaryWriter delegate for T.
        /// </summary>
        private static SummaryWriterDelegate<T> BuildSummaryWriter()
        {
            CodeNamespace ns = BuildNamespace();
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(ns);

            CompilerResults compilerResults = DynamicCodeGeneratorHelper.Compile(codeCompileUnit, KeepTempFiles, "Microsoft.CSharp.dll", "System.Core.dll");

            Type summaryWriterType = compilerResults.CompiledAssembly.GetType(FQDynamicClassName);

            return
                (SummaryWriterDelegate<T>)
                Delegate.CreateDelegate(typeof(SummaryWriterDelegate<T>), summaryWriterType, "GetSummary");
        }

        /// <summary>
        /// Creates a populated namespace containing code for a class
        /// that has a GetSummary method for T.
        /// </summary>
        private static CodeNamespace BuildNamespace()
        {
            CodeNamespace ns = new CodeNamespace(DynamicNamespace);
            CodeTypeDeclaration cls = new CodeTypeDeclaration(DynamicClassName);
            ns.Types.Add(cls);

            cls.BaseTypes.Add(typeof(EntitySummaryWriterBase));

            //public static string GetSummary(T entity)
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            method.Name = "GetSummary";
            method.Parameters.Add(new CodeParameterDeclarationExpression(_type, "entity"));
            method.ReturnType = new CodeTypeReference(typeof(string));
            cls.Members.Add(method);

            //StringBuilder sb = new StringBuilder()
            CodeVariableDeclarationStatement initialiseStringBuilder = new CodeVariableDeclarationStatement(
                typeof(StringBuilder), "sb", new CodeObjectCreateExpression(typeof(StringBuilder)));
            method.Statements.Add(initialiseStringBuilder);

            bool first = true;

            CodeVariableReferenceExpression entity = new CodeVariableReferenceExpression("entity");

            foreach (PropertyInfo prop in _type.GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                if (prop.GetIndexParameters().Any())
                    continue;

                if (prop.GetCustomAttributes(typeof(SummaryIgnoreAttribute), false).Length > 0)
                    continue;

                if (!first)
                {
                    //sb.Append(";");
                    CodeMethodInvokeExpression addSemiColon = new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("sb"), "Append", new CodePrimitiveExpression(';'));
                    method.Statements.Add(addSemiColon);
                }

                //AppendPair(<propertyName>, <propertyValue>, sb) (calls the AppendPair method on the base class for the property
                CodeMethodInvokeExpression addPropertyValue =
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(DynamicClassName),
                        "AppendPair",
                        new CodePrimitiveExpression(prop.Name),
                        new CodePropertyReferenceExpression(entity, prop.Name),
                        new CodeVariableReferenceExpression("sb"));

                method.Statements.Add(addPropertyValue);

                first = false;
            }

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("sb"), "ToString"));

            method.Statements.Add(returnStatement);

            return ns;
        }
    }
}