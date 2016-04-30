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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TemplateCodeFixProvider)), Shared]
    public class TemplateCodeFixProvider : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = syntaxRoot.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: _s_codeFixTitle,
                    createChangedDocument: c => PreProcessTemplate(context.Document, syntaxRoot, declaration, c),
                    equivalenceKey: _s_codeFixTitle),
                diagnostic);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            TemplateDiagnosticIds.TemplateWasNotPreprocessed
        );

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task<Document> PreProcessTemplate(
            Document document, 
            SyntaxNode syntaxRoot,
            ClassDeclarationSyntax handCodedPartial, 
            CancellationToken cancellation)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellation);

            var semanticModel = await document.GetSemanticModelAsync(cancellation);
            var typeSymbol = semanticModel.GetDeclaredSymbol(handCodedPartial, cancellation);
            var interfaceSymbol = editor.SemanticModel.Compilation.GetTypeByMetadataName(typeof(IPatternTemplate).FullName);
            var applyMethodSymbol = interfaceSymbol.GetMembers(nameof(IPatternTemplate.Apply)).OfType<IMethodSymbol>().First();
            var applyMethodDeclaration = DeclareExplicitInterfaceImplementationMethod(editor.Generator, interfaceSymbol, applyMethodSymbol);

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

            editor.InsertAfter(handCodedPartial, generatedPartial);

            if (!handCodedPartial.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                var newHandCodedPartial = (ClassDeclarationSyntax)editor.Generator.WithModifiers(handCodedPartial, DeclarationModifiers.Partial);
                editor.ReplaceNode(handCodedPartial, newHandCodedPartial);
            }

            return editor.GetChangedDocument();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static MethodDeclarationSyntax DeclareExplicitInterfaceImplementationMethod(SyntaxGenerator generator, ITypeSymbol interfaceType, IMethodSymbol interfaceMethod)
        {
            var returnType = interfaceMethod.ReturnType.IsSystemVoid() 
                ? PredefinedType(Token(SyntaxKind.VoidKeyword))
                : generator.TypeExpression(interfaceMethod.ReturnType);

            var declaration = MethodDeclaration((TypeSyntax)returnType, Identifier(interfaceMethod.Name));
            declaration = declaration.WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(interfaceType.FullNameSyntax()));
            declaration = declaration.WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(
                interfaceMethod.Parameters.Select(p => generator.ParameterDeclaration(p)).Cast<ParameterSyntax>()
            )));

            return declaration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_codeFixTitle = "Preprocess this template";
    }
}