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
        private readonly IPatternCompiler _compiler;
        private readonly bool _enableDebug;
        private readonly string _assemblyName;
        private readonly string _assemblyDirectory;
        private readonly object _syncRoot = new object();
        private ImmutableHashSet<string> _referencePaths;
        private ImmutableArray<PatternWriter> _writers;
        private ImmutableArray<Assembly> _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(string assemblyName)
            : this(new InProcessPatternCompiler(), assemblyName, assemblyDirectory: null, enableDebug: false)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(string assemblyName, params Assembly[] preloadedAssemblies)
            : this(new InProcessPatternCompiler(), assemblyName, assemblyDirectory: null, enableDebug: false, preloadedAssemblies: preloadedAssemblies)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(IPatternCompiler compiler, string assemblyName)
            : this(compiler, assemblyName, assemblyDirectory: null, enableDebug: false)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(IPatternCompiler compiler, string assemblyName, params Assembly[] preloadedAssemblies)
            : this(compiler, assemblyName, assemblyDirectory: null, enableDebug: false, preloadedAssemblies: preloadedAssemblies)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary(IPatternCompiler compiler, string assemblyName, string assemblyDirectory, bool enableDebug, params Assembly[] preloadedAssemblies)
        {
            _compiler = compiler;
            _enableDebug = enableDebug;
            _assemblyName = assemblyName;
            _assemblyDirectory = assemblyDirectory;
            _referencePaths = ImmutableHashSet.Create<string>(StringComparer.InvariantCultureIgnoreCase);
            _writers = ImmutableArray.Create<PatternWriter>();
            _assemblies = ImmutableArray.Create<Assembly>(preloadedAssemblies);

            if (!string.IsNullOrEmpty(assemblyDirectory))
            {
                Directory.CreateDirectory(assemblyDirectory);
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
                _referencePaths = _referencePaths.Add(assembly.Location);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureMetadataReference(Type type)
        {
            lock (_syncRoot)
            {
                _referencePaths = _referencePaths.Add(type.Assembly.Location);
            }
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
                    File.WriteAllBytes(Path.Combine(_assemblyDirectory, uniqueAssemblyName + ".dll"), dllBytes);
                    File.WriteAllBytes(Path.Combine(_assemblyDirectory, uniqueAssemblyName + ".pdb"), pdbBytes);
                    compiledAssembly = Assembly.LoadFrom(Path.Combine(_assemblyDirectory, uniqueAssemblyName + ".dll"));
                }
                else
                {
                    compiledAssembly = Assembly.Load(dllBytes);
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
                Console.WriteLine($"PERF >> PatternLibrary::CompileAssembly # 1 >> {clock.ElapsedMilliseconds} ms");

                var sourceCode = CreateSourceCode(uniqueAssemblyName, allMembers);

                Console.WriteLine($"PERF >> PatternLibrary::CompileAssembly # 2 >> {clock.ElapsedMilliseconds} ms");

                CompileOrThrow(uniqueAssemblyName, out dllBytes, out pdbBytes, sourceCode);

                Console.WriteLine($"PERF >> PatternLibrary::CompileAssembly # 3 >> {clock.ElapsedMilliseconds} ms");
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
                File.WriteAllText(sourceFilePath, sourceCode);
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
