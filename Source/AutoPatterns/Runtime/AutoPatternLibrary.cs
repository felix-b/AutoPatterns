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
        private ImmutableArray<AutoPatternWriter> _writers;
        private ImmutableArray<Assembly> _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutoPatternLibrary(string assemblyName)
        {
            _assemblyName = assemblyName;
            _references = new MetadataReferenceCache();
            _writers = ImmutableArray.Create<AutoPatternWriter>();
            _assemblies = ImmutableArray.Create<Assembly>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CompileMembersWrittenSoFar()
        {
            lock (_syncRoot)
            {
                var uniqueAssemblyName = $"{_assemblyName}_{_assemblies.Length}";
                Assembly compiledAssembly;

                if (CompileAndLoadAssembly(_writers.ToArray(), uniqueAssemblyName, out compiledAssembly))
                {
                    _assemblies = _assemblies.Add(compiledAssembly);
                    return true;
                }

                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImmutableArray<AutoPatternWriter> Writers => _writers;
        public ImmutableArray<Assembly> Assemblies => _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void AddWriter(AutoPatternWriter writer)
        {
            lock (_syncRoot)
            {
                _writers = _writers.Add(writer);
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

        private bool CompileAssembly(AutoPatternWriter[] writers, string assemblyName, out byte[] assemblyBytes)
        {
            var allMembers = new List<MemberDeclarationSyntax>();

            foreach (var writer in writers)
            {
                TakeMembersFromWriter(writer, allMembers);
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

        private bool CompileAndLoadAssembly(AutoPatternWriter[] compilers, string assemblyName, out Assembly compiledAssembly)
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

        private void TakeMembersFromWriter(AutoPatternWriter writer, List<MemberDeclarationSyntax> allMembers)
        {
            var members = writer.TakeWrittenMembers();
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
