using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.DesignTime
{
    public abstract class DesignTimeUnitTestBase
    {
        protected const string DefaultFilePathPrefix = "Test";
        protected const string CSharpDefaultFileExt = "cs";
        protected const string TestProjectName = "TestProject";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static DesignTimeUnitTestBase()
        {
            var platform = TestLibrary.Platform;
            var systemAssemblyFolder = Path.GetDirectoryName(platform.AssemblyLocation(typeof(object).GetTypeInfo().Assembly));

            CorlibReference = MetadataReference.CreateFromFile(Path.Combine(systemAssemblyFolder, "mscorlib.dll"));
            SystemRuntimeReference = MetadataReference.CreateFromFile(Path.Combine(systemAssemblyFolder, "System.Runtime.dll"));
            SystemCoreReference = MetadataReference.CreateFromFile(platform.AssemblyLocation(typeof(Enumerable).GetTypeInfo().Assembly));
            CSharpSymbolsReference = MetadataReference.CreateFromFile(platform.AssemblyLocation(typeof(CSharpCompilation).GetTypeInfo().Assembly));
            CodeAnalysisReference = MetadataReference.CreateFromFile(platform.AssemblyLocation(typeof(Compilation).GetTypeInfo().Assembly));
            AutoPatternsReference = MetadataReference.CreateFromFile(platform.AssemblyLocation(typeof(MetaProgram).GetTypeInfo().Assembly));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static MetadataReference CorlibReference { get; private set; }
        protected static MetadataReference SystemRuntimeReference { get; private set; }
        protected static MetadataReference SystemCoreReference { get; private set; }
        protected static MetadataReference CSharpSymbolsReference { get; private set; }
        protected static MetadataReference CodeAnalysisReference { get; private set; }
        protected static MetadataReference AutoPatternsReference { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Helpers available to inherited test classes

        protected Document CreateDocument(string source)
        {
            return CreateProject(new[] { source }).Documents.First();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Document CreateDocumentVerifyCompilable(string source)
        {
            var document = CreateProject(new[] { source }).Documents.First();

            GetCompilerDiagnostics(document)
                .Where(d => d.Severity >= DiagnosticSeverity.Info)
                .ShouldBeEmpty("The document fails to compile.");

            return document;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Document[] CreateDocuments(string[] sources)
        {
            var project = CreateProject(sources);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                Assert.Fail("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string DocumentToReducedFormattedString(Document document)
        {
            var simplifiedDoc = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
            var root = simplifiedDoc.GetSyntaxRootAsync().Result;
            root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
            return root.GetText().ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Diagnostic[] AnalyzeDocuments(DiagnosticAnalyzer analyzer, params Document[] documents)
        {
            var projects = new HashSet<Project>();
            foreach (var document in documents)
            {
                projects.Add(document.Project);
            }

            var diagnostics = new List<Diagnostic>();
            foreach (var project in projects)
            {
                var compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
                var diags = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
                foreach (var diag in diags)
                {
                    if (diag.Location == Location.None || diag.Location.IsInMetadata)
                    {
                        diagnostics.Add(diag);
                    }
                    else
                    {
                        for (int i = 0; i < documents.Length; i++)
                        {
                            var document = documents[i];
                            var tree = document.GetSyntaxTreeAsync().Result;
                            if (tree == diag.Location.SourceTree)
                            {
                                diagnostics.Add(diag);
                            }
                        }
                    }
                }
            }

            var results = SortDiagnostics(diagnostics);
            diagnostics.Clear();
            return results;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IEnumerable<Diagnostic> GetCompilerDiagnostics(Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Document ApplyCodeFix(Document document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void RunDiagnosticsAndCodefixEndToEnd(
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider,
            string originalSource,
            string expectedFinalSource,
            int? codeFixIndex = null,
            bool allowNewCompilerDiagnostics = false)
        {
            var document = CreateDocument(originalSource);
            var analyzerDiagnostics = AnalyzeDocuments(analyzer, new[] { document });
            var compilerDiagnostics = GetCompilerDiagnostics(document);
            var attempts = analyzerDiagnostics.Length;

            for (int i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    document = ApplyCodeFix(document, actions.ElementAt((int)codeFixIndex));
                    break;
                }

                document = ApplyCodeFix(document, actions.ElementAt(0));
                analyzerDiagnostics = AnalyzeDocuments(analyzer, new[] { document });

                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                //check if applying the code fix introduced any new compiler diagnostics
                if (!allowNewCompilerDiagnostics)
                {
                    newCompilerDiagnostics.Any(d => d.Severity >= DiagnosticSeverity.Info).ShouldBeFalse(() => {
                        document = document.WithSyntaxRoot(Formatter.Format(
                            document.GetSyntaxRootAsync().Result,
                            Formatter.Annotation,
                            document.Project.Solution.Workspace));
                        newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                        return (
                            $"Fix introduced new compiler diagnostics:\r\n{String.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString()))}\r\n" +
                            $"\r\nNew document:\r\n{document.GetSyntaxRootAsync().Result.ToFullString()}\r\n");
                    });
                }

                //check if there are analyzer diagnostics left after the code fix
                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
            }

            //after applying all of the code fixes, compare the resulting string to the inputted one
            var actualFinalSource = DocumentToReducedFormattedString(document);
            actualFinalSource.ShouldBeSourceCode(expectedFinalSource);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string NormalizeSourceCode(string code)
        {
            var syntax = CSharpSyntaxTree.ParseText(code);
            var normalizedSyntax = CSharpSyntaxTree.Create((CSharpSyntaxNode)syntax.GetRoot().NormalizeWhitespace());
            return normalizedSyntax.GetText().ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string CompleteCompilationUnitSource(string insideNamespaceCode)
        {
            return NormalizeSourceCode(@"
                using AutoPatterns; 
                using AutoPatterns.Runtime;
                using Microsoft.CodeAnalysis.CSharp;
                using Microsoft.CodeAnalysis.CSharp.Syntax;
                using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
                namespace MyNS 
                {" + "\r\n" +
                insideNamespaceCode + 
                "}\r\n");
        }
        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static Project CreateProject(string[] sources)
        {
            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, SystemRuntimeReference)
                .AddMetadataReference(projectId, SystemCoreReference)
                .AddMetadataReference(projectId, CSharpSymbolsReference)
                .AddMetadataReference(projectId, CodeAnalysisReference)
                .AddMetadataReference(projectId, AutoPatternsReference);

            int count = 0;
            foreach (var source in sources)
            {
                var newFileName = DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }

            return solution.GetProject(projectId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Compare two collections of Diagnostics,and return a list of any new diagnostics that appear only in the second collection.
        /// Note: Considers Diagnostics to be the same if they have the same Ids.  In the case of multiple diagnostics with the same Id in a row,
        /// this method may not necessarily return the new one.
        /// </summary>
        /// <param name="diagnostics">The Diagnostics that existed in the code before the CodeFix was applied</param>
        /// <param name="newDiagnostics">The Diagnostics that exist in the code after the CodeFix was applied</param>
        /// <returns>A list of Diagnostics that only surfaced in the code after the CodeFix was applied</returns>
        internal static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
        {
            var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
            var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

            int oldIndex = 0;
            int newIndex = 0;

            while (newIndex < newArray.Length)
            {
                if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
                {
                    ++oldIndex;
                    ++newIndex;
                }
                else
                {
                    yield return newArray[newIndex++];
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Location where the diagnostic appears, as determined by path, line number, and column number.
        /// </summary>
        public struct DiagnosticResultLocation
        {
            public DiagnosticResultLocation(string path, int line = -1, int column = -1)
            {
                if (line < -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
                }

                if (column < -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
                }

                this.Path = path;
                this.Line = line;
                this.Column = column;
            }

            public string Path { get; }
            public int Line { get; }
            public int Column { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Struct that stores information about a Diagnostic appearing in a source
        /// </summary>
        public struct DiagnosticExpectation
        {
            private DiagnosticResultLocation[] locations;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DiagnosticExpectation(DiagnosticSeverity severity, string id, string message, params DiagnosticResultLocation[] locations)
            {
                Severity = severity;
                Id = id;
                Message = message;
                this.locations = locations;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DiagnosticExpectation(DiagnosticSeverity severity, string id, string message, string atPath, int atLine, int atColumn = -1)
                : this(severity, id, message, new DiagnosticResultLocation[] {
                    new DiagnosticResultLocation(atPath, atLine, atColumn), 
                })
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DiagnosticExpectation(DiagnosticSeverity severity, string id, string atPath, int atLine, int atColumn = -1)
                : this(severity, id, null, new DiagnosticResultLocation[] {
                    new DiagnosticResultLocation(atPath, atLine, atColumn),
                })
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DiagnosticResultLocation[] Locations
            {
                get
                {
                    if (this.locations == null)
                    {
                        this.locations = new DiagnosticResultLocation[] { };
                    }
                    return this.locations;
                }

                set
                {
                    this.locations = value;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DiagnosticSeverity Severity { get; set; }
            public string Id { get; set; }
            public string Message { get; set; }
            public string Path
            {
                get
                {
                    return this.Locations.Length > 0 ? this.Locations[0].Path : "";
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Line
            {
                get
                {
                    return this.Locations.Length > 0 ? this.Locations[0].Line : -1;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Column
            {
                get
                {
                    return this.Locations.Length > 0 ? this.Locations[0].Column : -1;
                }
            }
        }
    }
}

#if false
        /// <summary>
        /// Given classes in the form of strings, their language, and an IDiagnosticAnlayzer to apply to it, return the 
        /// diagnostics found in the string after converting it to a document.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <param name="analyzer">The analyzer to be run on the sources</param>
        /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
        protected static Diagnostic[] GetSortedDiagnostics(string[] sources, DiagnosticAnalyzer analyzer)
        {
            return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources, LanguageNames.CSharp));
        }

        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on</param>
        /// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
        protected void VerifyCSharpDiagnostic(string source, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(new[] { source }, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected);
        }


        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        protected void VerifyCSharpDiagnostic(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(sources, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// Called to test a VB DiagnosticAnalyzer when applied on the inputted strings as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        protected void VerifyBasicDiagnostic(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(sources, LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// General method that gets a collection of actual diagnostics found in the source after the analyzer is run, 
        /// then verifies each of them.
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="language">The language of the classes represented by the source strings</param>
        /// <param name="analyzer">The analyzer to be run on the source code</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        private void VerifyDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
        {
            var diagnostics = GetSortedDiagnostics(sources, language, analyzer);
            VerifyDiagnosticResults(diagnostics, analyzer, expected);
        }
        /// <summary>
        /// Called to test a C# codefix when applied on the inputted string as a source
        /// </summary>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        protected void VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyFix(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
        }


#endif



