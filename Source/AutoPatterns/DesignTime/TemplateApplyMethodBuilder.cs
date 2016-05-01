using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.DesignTime
{
    public class TemplateApplyMethodBuilder
    {
        private readonly INamedTypeSymbol _templateClassSymbol;
        private readonly DocumentEditor _editor;
        private readonly List<StatementSyntax> _statements;
        private MethodDeclarationSyntax _applyMethodSyntax;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TemplateApplyMethodBuilder(INamedTypeSymbol templateClassSymbol, DocumentEditor editor)
        {
            _templateClassSymbol = templateClassSymbol;
            _editor = editor;
            _statements = new List<StatementSyntax>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildApplyMethod()
        {
            _applyMethodSyntax = DeclareApplyMethod();
            _applyMethodSyntax = _applyMethodSyntax.WithBody(Block(_statements));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodDeclarationSyntax ApplyMethodSyntax => _applyMethodSyntax;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MethodDeclarationSyntax DeclareApplyMethod()
        {
            var interfaceSymbol = _editor.SemanticModel.Compilation.GetTypeByMetadataName(typeof(IPatternTemplate).FullName);
            var applyMethodSymbol = interfaceSymbol.GetMembers(nameof(IPatternTemplate.Apply)).OfType<IMethodSymbol>().Single();
            var voidReturnType = PredefinedType(Token(SyntaxKind.VoidKeyword));
            var declaration = MethodDeclaration(voidReturnType, Identifier(applyMethodSymbol.Name));

            declaration = declaration.WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(interfaceSymbol.FullNameSyntax()));
            declaration = declaration.WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(
                applyMethodSymbol.Parameters.Select(p => _editor.Generator.ParameterDeclaration(p)).Cast<ParameterSyntax>()
            )));

            return declaration;
        }
    }
}
