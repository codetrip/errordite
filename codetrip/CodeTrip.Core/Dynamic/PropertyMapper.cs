using System;
using System.CodeDom;
using System.Linq;
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Dynamic
{
    /// <summary>
    /// Allows values in properties with identical name and type to be automatically transferred
    /// from source to destination objects.
    /// </summary>
    public static class PropertyMapper
    {
        /// <summary>
        /// Copies the values from all public readable properties in source object to any 
        /// public, writable properties with the same name and type in the destination object.
        /// </summary>
        public static void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            PropertyMapper<TSource, TDestination>.Map(source, destination);
        }
    }

    /// <summary>
    /// Allows values in properties with identical name and type to be automatically transferred
    /// from source to destination objects.
    /// </summary>
    public static class PropertyMapper<TSource, TDestination> 
    {
        private const string DynamicNamespace = "ProductionProfiler.Core.Dynamic";
        private const bool KeepTempFiles = true;
        private static readonly string DynamicClassName = "IdenticalPropertySetter_" + Guid.NewGuid().ToString().Replace("-", string.Empty);

        private static readonly string FQDynamicClassName = DynamicNamespace + "." + DynamicClassName;
        private static readonly Type _sourceType = typeof(TSource);
        private static readonly Type _destinationType = typeof (TDestination);
        private static Action<TSource, TDestination> _propertySetterDelegate;
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Copies the values from all public readable properties in source object to any 
        /// public, writable properties with the same name and type in the destination object.
        /// </summary>
        public static void Map(TSource source, TDestination destination)
        {
            if (_propertySetterDelegate == null)
            {
                lock (_syncLock)
                {
                    if (_propertySetterDelegate == null)
                        _propertySetterDelegate = BuildSetter();
                }
            }

            _propertySetterDelegate(source, destination);
        }

        private static Action<TSource, TDestination> BuildSetter()
        {
            var ns = BuildNamespace();
            var codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(ns);

            var compilerResults = DynamicCodeGeneratorHelper.Compile(codeCompileUnit, KeepTempFiles, "System.ComponentModel.DataAnnotations.dll");
            var dynamicType = compilerResults.CompiledAssembly.GetType(FQDynamicClassName);

            return (Action<TSource, TDestination>)Delegate.CreateDelegate(typeof(Action<TSource, TDestination>), dynamicType, "Map");
        }

        private static CodeNamespace BuildNamespace()
        {
            var ns = new CodeNamespace(DynamicNamespace);
            var cls = new CodeTypeDeclaration(DynamicClassName);
            ns.Types.Add(cls);

            //public static string Map(TSource source, TDestination destination)
            var method = new CodeMemberMethod
                             {
                                 Attributes = MemberAttributes.Static | MemberAttributes.Public,
                                 Name = "Map"
                             };
            method.Parameters.Add(new CodeParameterDeclarationExpression(_sourceType, "source"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(_destinationType, "destination"));
            method.ReturnType = null;
            cls.Members.Add(method);

            var destProps = _destinationType.GetTypeWritableProperties().ToList();

            //destination.Prop = source.Prop
            foreach (var assignment in from sp in _sourceType.GetTypeReadableProperties()
                                       where destProps.Any(dp => dp.Name == sp.Name && dp.PropertyType == sp.PropertyType)
                                       select new CodeAssignStatement(
                                           new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destination"), sp.Name), 
                                           new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("source"), sp.Name)))
            {
                method.Statements.Add(assignment);
            }

            return ns;

        }
    }


}