﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace AutoPatterns.Runtime
{
    public class InProcessPatternCompiler : IPatternCompiler
    {
        private readonly ReferenceCache _referenceCache = new ReferenceCache();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CompileAssembly(
            string assemblyName,
            string sourceCode,
            string[] references,
            bool enableDebug,
            out byte[] dllBytes,
            out byte[] pdbBytes,
            out string[] errors)
        {
            var context = new CompilationContext {
                AssemblyName = assemblyName,
                SourceCode = sourceCode,
                ReferencePaths = references,
                EnableDebug = enableDebug
            };

            LoadReferences(context);
            ParseSourceCode(context);
            CreateCompilation(context);
            EmitAssembly(context);

            dllBytes = context.DllBytes;
            pdbBytes = context.PdbBytes;
            errors = context.Errors;
            return context.Success;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadReferences(CompilationContext context)
        {
            var references = new MetadataReference[context.ReferencePaths.Length];

            for (int i = 0; i < references.Length; i++)
            {
                references[i] = _referenceCache.EnsureReferenceCached(context.ReferencePaths[i]);
            }

            context.LoadedReferences = references;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ParseSourceCode(CompilationContext context)
        {
            context.ParsedSyntax = CSharpSyntaxTree.ParseText(context.SourceCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateCompilation(CompilationContext context)
        {
            context.Compilation = CSharpCompilation
                .Create(context.AssemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(context.LoadedReferences)
                .AddSyntaxTrees(context.ParsedSyntax);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EmitAssembly(CompilationContext context)
        {
            using (var dllStream = new MemoryStream())// (capacity: 16384))
            {
                using (var pdbStream = context.EnableDebug ? new MemoryStream() : null)
                {
                    //var clock = Stopwatch.StartNew();
                    EmitResult result = context.Compilation.Emit(dllStream, pdbStream);
                    //Console.WriteLine(">> COMPILE TIME, ms = {0}", clock.ElapsedMilliseconds);

                    context.Success = result.Success;

                    if (!result.Success)
                    {
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(IsErrorOrWarning);
                        context.Errors = failures.Select(f => f.ToString()).ToArray();
                    }

                    context.DllBytes = dllStream.ToArray();
                    context.PdbBytes = pdbStream?.ToArray();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsErrorOrWarning(Diagnostic diagnostic)
        {
            return (diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CompilationContext
        {
            public string AssemblyName { get; set; }
            public string SourceCode { get; set; }
            public string[] ReferencePaths { get; set; }
            public bool EnableDebug { get; set; }
            public MetadataReference[] LoadedReferences { get; set; }
            public SyntaxTree ParsedSyntax { get; set; }
            public CSharpCompilation Compilation { get; set; }
            public byte[] DllBytes { get; set; }
            public byte[] PdbBytes { get; set; }
            public string[] Errors { get; set; }
            public bool Success { get; set; }
        }
    }
}
