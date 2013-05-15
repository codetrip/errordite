using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace Errordite.Core.Dynamic
{
    /// <summary>
    /// Base class for the dynamically generated SummaryWriter classes.
    /// </summary>
    public abstract class RuntimeXmlSerialiserBase
    {
        protected static void AddDictionary<TKey, TValue>(string name, IDictionary<TKey, TValue> value, XElement xml)
        {
            XElement nestedType = new XElement(RuntimeSerialisationHelper.CleanName(name));

            if (value == null)
            {
                AddSimpleType(name, null, nestedType);
                return;
            }

            foreach (KeyValuePair<TKey, TValue> pair in value)
            {
                nestedType.Add(RuntimeSerialisationHelper.IsSimpleType(typeof(TValue))
                    ? new XElement(RuntimeSerialisationHelper.CleanName(pair.Key.ToString()), pair.Value.ToString())
                    : new XElement(RuntimeSerialisationHelper.CleanName(pair.Key.ToString()), XmlSummaryWriter<TValue>.GetSummary(pair.Value)));
            }

            xml.Add(nestedType);
        }

        protected static void AddDefault<T>(string name, T value, XElement xml)
        {
            XElement nestedType = new XElement(RuntimeSerialisationHelper.CleanName(name));

            if (value is IEnumerable)
            {
                foreach (object enumerableProperty in (IEnumerable)value)
                {
                    nestedType.Add(new XElement(RuntimeSerialisationHelper.CleanName(enumerableProperty.GetType().Name), enumerableProperty.ToString()));
                }
            }
            else
            {
                if (RuntimeSerialisationHelper.IsSimpleType(typeof(T)))
                    AddSimpleType(name, value, nestedType);
                else
                    nestedType.Add(XmlSummaryWriter<T>.GetSummary(value));
            }

            xml.Add(nestedType);
        }

        protected static void AddSingleParameterGenericType<T, T2>(string name, object value, XElement xml)
        {
            XElement nestedType = new XElement(RuntimeSerialisationHelper.CleanName(name));

            if (value == null)
            {
                AddSimpleType(name, null, nestedType);
                return;
            }

            if (value is IEnumerable && typeof(T).IsGenericType)
            {
                foreach (T2 enumerableProperty in (IEnumerable)value)
                {
                    nestedType.Add(RuntimeSerialisationHelper.IsSimpleType(typeof(T2))
                        ? new XElement(RuntimeSerialisationHelper.CleanName(enumerableProperty.GetType().Name), ((object)enumerableProperty ?? "null").ToString())
                        : XmlSummaryWriter<T2>.GetSummary(enumerableProperty));
                }
            }
            else if (value is IEnumerable)
            {
                foreach (T2 enumerableProperty in (IEnumerable)value)
                {
                    nestedType.Add(RuntimeSerialisationHelper.IsSimpleType(typeof(T2))
                        ? new XElement(RuntimeSerialisationHelper.CleanName(enumerableProperty.GetType().Name), enumerableProperty.ToString())
                        : XmlSummaryWriter<T2>.GetSummary(enumerableProperty));
                }
            }
            else if (value is T2)
            {
                nestedType.Add(XmlSummaryWriter<T2>.GetSummary((T2)value));
            }
            else if (value is T)
            {
                nestedType.Add(XmlSummaryWriter<T>.GetSummary((T)value));
            }

            if (nestedType.Elements().Count() > 0)
                xml.Add(nestedType);
        }

        protected static void AddSimpleType(string name, object value, XElement xml)
        {
            xml.Add(new XElement(RuntimeSerialisationHelper.CleanName(name), (value ?? "null").ToString()));
        }
    }

    /// <summary>
    /// Delegate for a method that writes a Summary string for an entity of type T.
    /// </summary>
    public delegate XElement XmlSummaryWriterDelegate<in T>(T entity);

    public static class RuntimeXmlSerialiser
    {
        public static XElement Serialise<T>(T entity)
        {
            return XmlSummaryWriter<T>.GetSummary(entity);
        }
    }

    /// <summary>
    /// Writes a summary for an entity in an efficient manner (dynamically writes
    /// and compiles a class then assigns a delegate to a member variable for use in the future.
    /// </summary>
    public static class XmlSummaryWriter<T>
    {
        #region Private Fields
        private const string DynamicNamespace = "ErrorDite.Dynamic";
        private const bool KeepTempFiles = false;
        private static readonly string _dynamicClassName = "RuntimeXmlSerialiser_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
        private static readonly string _fqDynamicClassName = DynamicNamespace + "." + _dynamicClassName;
        private static readonly object _syncLock = new object();
        private static readonly Type _type = typeof(T);
        private static XmlSummaryWriterDelegate<T> _summaryWriterDelegate;
        #endregion

        /// <summary>
        /// Assigns a delegate for creating a summary if it does not already exist
        /// then returns the results of calling the delegate.
        /// </summary>
        public static XElement GetSummary(T entity)
        {
            if (entity == null)
                return new XElement(RuntimeSerialisationHelper.CleanName(typeof(T).Name));

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
        private static XmlSummaryWriterDelegate<T> BuildSummaryWriter()
        {
            CodeNamespace ns = BuildNamespace();
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(ns);

            CompilerResults compilerResults = DynamicCodeGeneratorHelper.Compile(codeCompileUnit,
                KeepTempFiles,
                "System.dll",
                "System.Drawing.dll",
                "System.Xml.dll",
                "System.Xml.Linq.dll");

            Type summaryWriterType = compilerResults.CompiledAssembly.GetType(_fqDynamicClassName);

            return (XmlSummaryWriterDelegate<T>)Delegate.CreateDelegate(typeof(XmlSummaryWriterDelegate<T>), summaryWriterType, "Serialise");
        }

        /// <summary>
        /// Creates a populated namespace containing code for a class
        /// that has a GetSummary method for T.
        /// </summary>
        private static CodeNamespace BuildNamespace()
        {
            CodeNamespace ns = new CodeNamespace(DynamicNamespace);
            CodeTypeDeclaration cls = new CodeTypeDeclaration(_dynamicClassName);
            ns.Types.Add(cls);

            cls.BaseTypes.Add(typeof(RuntimeXmlSerialiserBase));

            //public static string GetSummary(T entity)
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            method.Name = "Serialise";
            method.Parameters.Add(new CodeParameterDeclarationExpression(_type, "entity"));
            method.ReturnType = new CodeTypeReference(typeof(XElement));
            cls.Members.Add(method);

            //XElement xElement = new XElement()
            CodeVariableDeclarationStatement initXElement = new CodeVariableDeclarationStatement(
                typeof(XElement), "xElement",
                new CodeObjectCreateExpression(typeof(XElement),
                    new CodePrimitiveExpression(RuntimeSerialisationHelper.CleanName(typeof(T).Name))));

            method.Statements.Add(initXElement);

            CodeVariableReferenceExpression entity = new CodeVariableReferenceExpression("entity");

            foreach (PropertyInfo prop in _type.GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                if (prop.GetIndexParameters().Any())
                    continue;

                if (prop.GetCustomAttributes(typeof(RuntimeSerialisationIgnoreAttribute), false).Length > 0)
                    continue;

                if (RuntimeSerialisationHelper.IsSimpleType(prop.PropertyType))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(_dynamicClassName),
                            "AddSimpleType",
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.IsGenericType && prop.PropertyType.Name.Contains("Dictionary`2"))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddDictionary",
                                new[]
                                    {
                                        new CodeTypeReference(prop.PropertyType.GetGenericArguments()[0]),
                                        new CodeTypeReference(prop.PropertyType.GetGenericArguments()[1])
                                    }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.BaseType != null && prop.PropertyType.BaseType.Name.Contains("Dictionary`2"))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddDictionary",
                                new[]
                                    {
                                        new CodeTypeReference(prop.PropertyType.BaseType.GetGenericArguments()[0]),
                                        new CodeTypeReference(prop.PropertyType.BaseType.GetGenericArguments()[1])
                                    }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.IsGenericType)
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddSingleParameterGenericType",
                                new[]
                                    {
                                        new CodeTypeReference(prop.PropertyType),
                                        new CodeTypeReference(prop.PropertyType.GetGenericArguments()[0])
                                    }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.BaseType != null && prop.PropertyType.BaseType == typeof(Array))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddSingleParameterGenericType",
                                new[]
                                    {
                                        new CodeTypeReference(prop.PropertyType),
                                        new CodeTypeReference(prop.PropertyType.FullName.Replace("[]", "")),
                                    }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.BaseType != null && prop.PropertyType.BaseType.IsGenericType)
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddSingleParameterGenericType",
                                new[]
                                    {
                                        new CodeTypeReference(prop.PropertyType),
                                        new CodeTypeReference(prop.PropertyType.BaseType.GetGenericArguments()[0]),
                                    }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddDefault",
                                new[]
                                    {
                                        new CodeTypeReference(prop.PropertyType),
                                    }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
            }

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeVariableReferenceExpression("xElement"));

            method.Statements.Add(returnStatement);

            return ns;
        }
    }

    internal static class RuntimeSerialisationHelper
    {
        public static bool IsSimpleType(Type typeToCheck)
        {
            return typeToCheck.IsValueType || typeToCheck.IsPrimitive || typeToCheck.Equals(typeof(string)) || typeToCheck.IsEnum;
        }

        public static string CleanName(IEnumerable<char> name)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in name)
            {
                if ((c >= 48 && c <= 57) || ((c >= 65 && c <= 90)) || ((c >= 97 && c <= 122)))
                    result.Append(c.ToString());
            }

            return result.ToString();
        }
    }
}