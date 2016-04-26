using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.DesignTime
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PatternTemplateCodeFixProvider)), Shared]
    public class PatternTemplateCodeFixProvider : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: _s_codeFixTitle,
                    createChangedDocument: c => PreProcessTemplate(context, declaration, c),
                    equivalenceKey: _s_codeFixTitle),
                diagnostic);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PatternTemplateDiagnosticAnalyzer.DiagnosticId);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task<Document> PreProcessTemplate(
            CodeFixContext context, 
            TypeDeclarationSyntax codedPartial, 
            CancellationToken cancellation)
        {
            var document = context.Document;
            var editor = await DocumentEditor.CreateAsync(document, cancellation);

            var typeSymbol = editor.SemanticModel.GetDeclaredSymbol(codedPartial, cancellation);
            var interfaceSymbol = editor.SemanticModel.Compilation.GetTypeByMetadataName(typeof(IPatternTemplate).FullName);
            var applyMethodSymbol = interfaceSymbol.GetMembers(nameof(IPatternTemplate.Apply)).OfType<IMethodSymbol>().First();
            var applyMethodDeclaration = DeclareExplicitInterfaceImplementationMethod(editor.Generator, interfaceSymbol, applyMethodSymbol);

            //applyMethodDeclaration = applyMethodDeclaration
            //    .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName(nameof(IPatternTemplate))));

            var generatedPartial = editor.Generator.ClassDeclaration(
                typeSymbol.Name, 
                accessibility: typeSymbol.DeclaredAccessibility,
                modifiers: DeclarationModifiers.Partial,
                interfaceTypes: new[] {
                    SyntaxFactory.ParseTypeName(typeof(IPatternTemplate).FullName)
                },
                members: new[] {
                    applyMethodDeclaration.WithBody(Block())
                });

            editor.InsertAfter(codedPartial, generatedPartial);
            return editor.GetChangedDocument();

            //// Compute new uppercase name.
            //var identifierToken = typeDecl.Identifier;
            //var newName = identifierToken.Text.ToCamelCase();

            //// Get the symbol representing the type to be renamed.
            //var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            //var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            //// Produce a new solution that has all references to that type renamed, including the declaration.
            //var originalSolution = document.Project.Solution;
            //var optionSet = originalSolution.Workspace.Options;
            //var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            //// Return the new solution with the now-uppercase type name.
            //return newSolution;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static MethodDeclarationSyntax DeclareExplicitInterfaceImplementationMethod(SyntaxGenerator generator, ITypeSymbol interfaceType, IMethodSymbol interfaceMethod)
        {
            var returnType = interfaceMethod.ReturnType.IsSystemVoid() 
                ? PredefinedType(Token(SyntaxKind.VoidKeyword))
                : generator.TypeExpression(interfaceMethod.ReturnType);

            var declaration = MethodDeclaration((TypeSyntax)returnType, Identifier(interfaceMethod.Name));
            declaration = declaration.WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName(interfaceType.Name)));

            return declaration;

            //method.Name, System.Linq.ImmutableArrayExtensions.Select<IParameterSymbol, SyntaxNode>(method.Parameters, (Func<IParameterSymbol, SyntaxNode>)(p => this.ParameterDeclaration(p, (SyntaxNode)null))), (IEnumerable<string>)null, ITypeSymbolExtensions.IsSystemVoid(method.ReturnType) ? (SyntaxNode)null : this.TypeExpression(method.ReturnType), method.DeclaredAccessibility, DeclarationModifiers.From((ISymbol)method), statements);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_codeFixTitle = "Preprocess this template";
    }
}