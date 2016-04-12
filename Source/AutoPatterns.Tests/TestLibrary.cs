using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess;
using AutoPatterns.Runtime;
using NUnit.Framework;

namespace AutoPatterns.Tests
{
    public static class TestLibrary
    {
        private static Func<IPatternCompiler> _s_compilerFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UseRemoteCompilerService(int tcpPortNumber = 50555)
        {
            _s_compilerFactory = () => new RemotePatternCompiler(tcpPortNumber);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UseInProcessCompiler()
        {
            _s_compilerFactory = () => new InProcessPatternCompiler();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IPatternCompiler CreateCompiler()
        {
            return _s_compilerFactory();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PatternLibrary CreateLibrary(string assemblyName, params Assembly[] preloadedAssemblies)
        {
            return new PatternLibrary(
                _s_compilerFactory(), 
                assemblyName, 
                assemblyDirectory: null, 
                enableDebug: false,
                preloadedAssemblies: preloadedAssemblies);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PatternLibrary CreateLibraryWithDebug(string assemblyName, params Assembly[] preloadedAssemblies)
        {
            return new PatternLibrary(
                _s_compilerFactory(), 
                assemblyName, 
                assemblyDirectory: TestContext.CurrentContext.TestDirectory, 
                enableDebug: true,
                preloadedAssemblies: preloadedAssemblies);
        }
    }
}
