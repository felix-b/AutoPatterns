using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPatterns.Runtime
{
    public interface IPatternCompiler
    {
        bool CompileAssembly(
            string assemblyName, 
            string sourceCode, 
            string[] references,
            bool enableDebug,
            out byte[] dllBytes,
            out byte[] pdbBytes,
            out string[] errors);
    }
}
