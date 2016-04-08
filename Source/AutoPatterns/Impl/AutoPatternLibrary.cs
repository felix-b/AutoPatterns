using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace AutoPatterns.Impl
{
    public class AutoPatternLibrary
    {
        private readonly string _assemblyName;
        private readonly object _syncRoot = new object();
        private ImmutableArray<AutoPatternCompiler> _compilers;
        private ImmutableArray<Assembly> _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutoPatternLibrary(string assemblyName)
        {
            _assemblyName = assemblyName;
            _compilers = ImmutableArray.Create<AutoPatternCompiler>();
            _assemblies = ImmutableArray.Create<Assembly>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CompileAddedSyntaxes()
        {
            lock (_syncRoot)
            {
                var uniqueAssemblyName = $"{_assemblyName}_{_assemblies.Length}";
                Assembly compiledAssembly;

                if (AutoPatternCompiler.CompileAndLoadAssembly(_compilers.ToArray(), uniqueAssemblyName, out compiledAssembly))
                {
                    _assemblies = _assemblies.Add(compiledAssembly);
                    return true;
                }

                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImmutableArray<AutoPatternCompiler> Compilers => _compilers;
        public ImmutableArray<Assembly> Assemblies => _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void AddCompiler(AutoPatternCompiler compiler)
        {
            lock (_syncRoot)
            {
                _compilers = _compilers.Add(compiler);
            }
        }
    }
}
