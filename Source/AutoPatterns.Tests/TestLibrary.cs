using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.Runtime;
using NUnit.Framework;

namespace AutoPatterns.Tests
{
    public static class TestLibrary
    {
        private const string PlatformAssemblyName =
            #if NET45
            "AutoPatterns.Platform.Net45";
            #endif

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly IAutoPatternsPlatform _s_platform;
        private static Func<IPatternCompiler> _s_compilerFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static TestLibrary()
        {
            _s_platform = LoadPlatformImplementation();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UseRemoteCompilerService(int tcpPortNumber = 50555)
        {
            _s_compilerFactory = () => _s_platform.CreateRemoteCompiler(tcpPortNumber);
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
                _s_platform,
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
                _s_platform,
                _s_compilerFactory(), 
                assemblyName, 
                assemblyDirectory: TestContext.CurrentContext.TestDirectory, 
                enableDebug: true,
                preloadedAssemblies: preloadedAssemblies);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IAutoPatternsPlatform Platform => _s_platform;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IAutoPatternsPlatform LoadPlatformImplementation()
        {
            var platformAssemblyRef = new AssemblyName() {
                Name = PlatformAssemblyName
            };

            var platformAssembly = Assembly.Load(platformAssemblyRef);
            var platformType = platformAssembly.ExportedTypes.FirstOrDefault(t => typeof(IAutoPatternsPlatform).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));

            if (platformType == null)
            {
                throw new TypeLoadException($"Cannot find pltaform implementation in assembly '{platformAssemblyRef.Name}'.");
            }

            var platformInstance = (IAutoPatternsPlatform)Activator.CreateInstance(platformType);
            return platformInstance;
        }
    }
}
