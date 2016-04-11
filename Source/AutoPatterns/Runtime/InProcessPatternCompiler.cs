//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis;

//namespace AutoPatterns.Runtime
//{
//    internal class InProcessPatternCompiler : IPatternCompiler
//    {
//        private readonly ReferenceCache _referenceCache = new ReferenceCache();

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public InProcessPatternCompiler()
//        {
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public bool CompileAssembly(
//            string assemblyName,
//            string sourceCode,
//            string[] references,
//            bool enableDebug,
//            out byte[] dllBytes,
//            out byte[] pdbBytes,
//            out string[] errors)
//        {
//            var loadedReferences = LoadReferences(references);
//        }

//        private MetadataReference[] LoadReferences(string[] referencePaths)
//        {
//            _referenceCache.EnsureReference()
//        }
//    }
//}
