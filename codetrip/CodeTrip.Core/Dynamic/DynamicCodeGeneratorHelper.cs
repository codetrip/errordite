using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Linq;

namespace CodeTrip.Core.Dynamic
{
    public static class DynamicCodeGeneratorHelper
    {
        private static readonly AssembliesInBinDirectoryLocator _assembliesToReferenceLocator = new AssembliesInBinDirectoryLocator();
        
        private static string BuildExceptionMessage(CompilerResults compilerResults)
        {
            return compilerResults.Errors.Cast<CompilerError>().Aggregate("Compile errors:", (current, error) => current + (Environment.NewLine + error));
        }

        /// <summary>
        /// Compile some code.
        /// </summary>
        /// <param name="codeCompileUnit">The code to compile.</param>
        /// <param name="keepTempFiles">If true the temp cs files will be left in the temp directory.  This makes debugging easier.</param>
        /// <param name="extraAssemblyReferences">By default the compilation references everything in the bin directory of the 
        /// running application.  If there are extra assemblies to reference add their names here (ensure they end in .dll).  It is presumed that these assemblies
        /// will be in the GAC.</param>
        /// <remarks>If the extra assembly reference is required simply because of a reference on the type (e.g. property, method) then it is enough to pass the GAC'd assembly's</remarks>
        public static CompilerResults Compile(CodeCompileUnit codeCompileUnit, bool keepTempFiles, params string[] extraAssemblyReferences)
        {
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");

            string[] refedAssemblies;
                _assembliesToReferenceLocator.Locate(out refedAssemblies);

            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.AddRange(refedAssemblies);
            cp.ReferencedAssemblies.AddRange(extraAssemblyReferences);
            cp.GenerateInMemory = true;
            cp.TempFiles.KeepFiles = keepTempFiles;

            CompilerResults compilerResults = codeDomProvider.CompileAssemblyFromDom(cp, codeCompileUnit);

            if (compilerResults.Errors.HasErrors)
                throw new InvalidOperationException(BuildExceptionMessage(compilerResults));

            return compilerResults;
        }
    }
}
