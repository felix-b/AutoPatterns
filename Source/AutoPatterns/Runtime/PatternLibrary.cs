using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Runtime
{
    public class PatternLibrary
    {
        private readonly IAutoPatternsPlatform _platform;
        private readonly IPatternCompiler _compiler;
        private readonly bool _enableDebug;
        private readonly string _assemblyName;
        private readonly string _assemblyDirectory;
        private readonly object _syncRoot = new object();
        private ImmutableHashSet<string> _referencePaths;
        private ImmutableArray<PatternWriter> _writers;
        private ImmutableArray<Assembly> _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(IAutoPatternsPlatform platform, string assemblyName)
            : this(platform, new InProcessPatternCompiler(), assemblyName, assemblyDirectory: null, enableDebug: false)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(IAutoPatternsPlatform platform, string assemblyName, params Assembly[] preloadedAssemblies)
            : this(platform, new InProcessPatternCompiler(), assemblyName, assemblyDirectory: null, enableDebug: false, preloadedAssemblies: preloadedAssemblies)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(IAutoPatternsPlatform platform, IPatternCompiler compiler, string assemblyName)
            : this(platform, compiler, assemblyName, assemblyDirectory: null, enableDebug: false)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(IAutoPatternsPlatform platform, IPatternCompiler compiler, string assemblyName, params Assembly[] preloadedAssemblies)
            : this(platform, compiler, assemblyName, assemblyDirectory: null, enableDebug: false, preloadedAssemblies: preloadedAssemblies)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(
            IAutoPatternsPlatform platform, 
            IPatternCompiler compiler, 
            string assemblyName, 
            string assemblyDirectory, 
            bool enableDebug, 
            params Assembly[] preloadedAssemblies)
        {
            _platform = platform;
            _compiler = compiler;
            _enableDebug = enableDebug;
            _assemblyName = assemblyName;
            _assemblyDirectory = assemblyDirectory;
            _referencePaths = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase, GetSystemAssemblyReferences());
            _writers = ImmutableArray.Create<PatternWriter>();
            _assemblies = ImmutableArray.Create<Assembly>(preloadedAssemblies);

            if (!string.IsNullOrEmpty(assemblyDirectory))
            {
                _platform.DirectoryCreateDirectory(assemblyDirectory);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CompileMembersWrittenSoFar()
        {
            lock (_syncRoot)
            {
                var uniqueAssemblyName = $"{_assemblyName}_{_assemblies.Length}";
                Assembly compiledAssembly;

                if (CompileAndLoad(uniqueAssemblyName, out compiledAssembly))
                {
                    _assemblies = _assemblies.Add(compiledAssembly);
                    return true;
                }

                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAutoPatternsPlatform Platform => _platform;
        public ImmutableArray<PatternWriter> Writers => _writers;
        public ImmutableArray<Assembly> Assemblies => _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void AddWriter(PatternWriter writer)
        {
            lock (_syncRoot)
            {
                _writers = _writers.Add(writer);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureMetadataReference(Assembly assembly)
        {
            lock (_syncRoot)
            {
                _referencePaths = _referencePaths.Add(_platform.AssemblyLocation(assembly));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureMetadataReference(Type type)
        {
            lock (_syncRoot)
            {
                _referencePaths = _referencePaths.Add(_platform.AssemblyLocation(type.GetTypeInfo().Assembly));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string[] GetSystemAssemblyReferences()
        {
            var systemAssemblyFolder = Path.GetDirectoryName(_platform.AssemblyLocation(typeof(object).GetTypeInfo().Assembly));

             return new[] {
                 Path.Combine(systemAssemblyFolder, "mscorlib.dll"),
                 Path.Combine(systemAssemblyFolder, "System.Runtime.dll")
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool CompileAndLoad(string uniqueAssemblyName, out Assembly compiledAssembly)
        {
            byte[] dllBytes;
            byte[] pdbBytes;

            if (CompileAssembly(uniqueAssemblyName, out dllBytes, out pdbBytes))
            {
                if (_enableDebug)
                {
                    _platform.FileWriteAllBytes(Path.Combine(_assemblyDirectory, uniqueAssemblyName + ".dll"), dllBytes);
                    _platform.FileWriteAllBytes(Path.Combine(_assemblyDirectory, uniqueAssemblyName + ".pdb"), pdbBytes);
                    compiledAssembly = _platform.AssemblyLoadFrom(Path.Combine(_assemblyDirectory, uniqueAssemblyName + ".dll"));
                }
                else
                {
                    compiledAssembly = _platform.AssemblyLoad(dllBytes);
                }

                return true;
            }

            compiledAssembly = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool CompileAssembly(string uniqueAssemblyName, out byte[] dllBytes, out byte[] pdbBytes)
        {
            var clock = Stopwatch.StartNew();

            List<MemberDeclarationSyntax> allMembers;

            if (ListMembersToCompile(out allMembers))
            {
                _platform.ConsoleWriteLine($"PERF >> PatternLibrary::CompileAssembly # 1 >> {clock.ElapsedMilliseconds} ms");

                var sourceCode = CreateSourceCode(uniqueAssemblyName, allMembers);

                _platform.ConsoleWriteLine($"PERF >> PatternLibrary::CompileAssembly # 2 >> {clock.ElapsedMilliseconds} ms");

                CompileOrThrow(uniqueAssemblyName, out dllBytes, out pdbBytes, sourceCode);

                _platform.ConsoleWriteLine($"PERF >> PatternLibrary::CompileAssembly # 3 >> {clock.ElapsedMilliseconds} ms");
                return true;
            }

            dllBytes = null;
            pdbBytes = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string CreateSourceCode(string uniqueAssemblyName, List<MemberDeclarationSyntax> allMembers)
        {
            var sourceSyntaxTree = SyntaxTree(CompilationUnit().WithMembers(List(allMembers)).NormalizeWhitespace(indentation: "\t"));
            var sourceCode = sourceSyntaxTree.ToString();

            if (_enableDebug)
            {
                var sourceFilePath = Path.Combine(_assemblyDirectory,  uniqueAssemblyName + ".cs");
                _platform.FileWriteAllText(sourceFilePath, sourceCode);
                sourceCode = $"#line 1 \"{sourceFilePath}\"{Environment.NewLine}" + sourceCode;
            }
            return sourceCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ListMembersToCompile(out List<MemberDeclarationSyntax> allMembers)
        {
            allMembers = new List<MemberDeclarationSyntax>();
            var currentWriters = _writers;

            foreach (var writer in currentWriters)
            {
                TakeMembersFromWriter(writer, allMembers);
            }

            return (allMembers.Count > 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompileOrThrow(string uniqueAssemblyName, out byte[] dllBytes, out byte[] pdbBytes, string sourceCode)
        {
            string[] errors;
            var success = _compiler.CompileAssembly(
                uniqueAssemblyName, 
                sourceCode, 
                _referencePaths.ToArray(), 
                _enableDebug, 
                out dllBytes, 
                out pdbBytes, 
                out errors);

            if (!success)
            {
                throw new Exception("Compile failed:\r\n" + string.Join("\r\n", errors));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TakeMembersFromWriter(PatternWriter writer, List<MemberDeclarationSyntax> allMembers)
        {
            var members = writer.TakeMembersWrittenSoFar();
            allMembers.AddRange(members);
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private byte[] EmitAssemblyBytes(CSharpCompilation compilation)
        //{
        //    using (var output = new MemoryStream())// (capacity: 16384))
        //    {
        //        var clock = Stopwatch.StartNew();
        //        EmitResult result = compilation.Emit(output);//, options: options);
        //        Console.WriteLine(">> COMPILE TIME, ms = {0}", clock.ElapsedMilliseconds);

        //        if (!result.Success)
        //        {
        //            IEnumerable<Diagnostic> failures =
        //                result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

        //            throw new Exception(
        //                "Compile failed:" +
        //                System.Environment.NewLine +
        //                string.Join(System.Environment.NewLine, failures.Select(f => $"{f.Id}: {f.GetMessage()}")));
        //        }

        //        return output.ToArray();
        //    }
        //}
    }
}
