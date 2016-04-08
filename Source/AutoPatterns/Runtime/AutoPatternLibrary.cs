using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class AutoPatternLibrary
    {
        private readonly string _assemblyName;
        private readonly object _syncRoot = new object();
        private readonly MetadataReferenceCache _references;
        private ImmutableArray<AutoPatternCompiler> _compilers;
        private ImmutableArray<Assembly> _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutoPatternLibrary(string assemblyName)
        {
            _assemblyName = assemblyName;
            _references = new MetadataReferenceCache();
            _compilers = ImmutableArray.Create<AutoPatternCompiler>();
            _assemblies = ImmutableArray.Create<Assembly>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CompilePendingSyntaxes()
        {
            lock (_syncRoot)
            {
                var uniqueAssemblyName = $"{_assemblyName}_{_assemblies.Length}";
                Assembly compiledAssembly;

                if (CompileAndLoadAssembly(_compilers.ToArray(), uniqueAssemblyName, out compiledAssembly))
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureMetadataReference(Assembly assembly)
        {
            _references.EnsureReference(assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureMetadataReference(Type type)
        {
            _references.EnsureReference(type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool CompileAssembly(AutoPatternCompiler[] compilers, string assemblyName, out byte[] assemblyBytes)
        {
            var allMembers = new List<MemberDeclarationSyntax>();

            foreach (var compiler in compilers)
            {
                IncludeExportsFromCompiler(compiler, allMembers);
            }

            if (allMembers.Count == 0)
            {
                assemblyBytes = null;
                return false;
            }

            var syntaxTree = SyntaxTree(CompilationUnit()
                .WithMembers(List(allMembers))
                .NormalizeWhitespace());

            //Console.WriteLine(syntaxTree.ToString());

            var compilation = CSharpCompilation
                .Create(assemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(_references.GetAllReferences())
                .AddSyntaxTrees(syntaxTree);

            assemblyBytes = EmitAssemblyBytes(compilation);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool CompileAndLoadAssembly(AutoPatternCompiler[] compilers, string assemblyName, out Assembly compiledAssembly)
        {
            byte[] assemblyBytes;

            if (CompileAssembly(compilers, assemblyName, out assemblyBytes))
            {
                compiledAssembly = Assembly.Load(assemblyBytes);
                return true;
            }

            compiledAssembly = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncludeExportsFromCompiler(
            AutoPatternCompiler compiler,
            List<MemberDeclarationSyntax> allMembers)
        {
            MemberDeclarationSyntax[] members;
            compiler.TakeAllSyntax(out members);
            allMembers.AddRange(members);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private byte[] EmitAssemblyBytes(CSharpCompilation compilation)
        {
            using (var output = new MemoryStream())
            {
                EmitResult result = compilation.Emit(output);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures =
                        result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    throw new Exception(
                        "Compile failed:" +
                        System.Environment.NewLine +
                        string.Join(System.Environment.NewLine, failures.Select(f => $"{f.Id}: {f.GetMessage()}")));
                }

                return output.ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetDefaultClassName(TypeKey key)
        {
            return key.ToString();
        }
    }
}
