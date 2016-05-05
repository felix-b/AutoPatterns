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
using static AutoPatterns.DesignTime.ClassWriterClientWriter;

namespace AutoPatterns.DesignTime
{
    public class TemplateApplyMethodBuilder
    {
        private readonly SemanticModel _semanticModel;
        private readonly DocumentEditor _editor;
        private readonly ClassDeclarationSyntax _templateClassSyntax;
        private readonly INamedTypeSymbol _templateClassSymbol;
        private readonly List<StatementSyntax> _statements;
        private readonly ClassWriterClientWriter _writer;
        private MethodDeclarationSyntax _applyMethodSyntax;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TemplateApplyMethodBuilder(
            ClassDeclarationSyntax templateClassSyntax,
            INamedTypeSymbol templateClassSymbol, 
            DocumentEditor editor)
        {
            _templateClassSyntax = templateClassSyntax;
            _templateClassSymbol = templateClassSymbol;
            _editor = editor;
            _semanticModel = editor.SemanticModel;
            _statements = new List<StatementSyntax>();
            _writer = new ClassWriterClientWriter(_semanticModel, _statements);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildApplyMethod()
        {
            _applyMethodSyntax = DeclareApplyMethod();

            ExecuteImplementationPipeline();

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

            declaration = declaration.WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(ParseName(interfaceSymbol.Name)));
            declaration = declaration.WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(
                applyMethodSymbol.Parameters.Select(p => _editor.Generator.ParameterDeclaration(p)).Cast<ParameterSyntax>()
            )));

            return declaration;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteImplementationPipeline()
        {
            ImplementClassLevelAttributes();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementClassLevelAttributes()
        {
            var classLevelAttributes = _templateClassSyntax
                .AttributeLists
                .SelectMany(list => list.Attributes)
                .Where(attr => !_writer.IsMetaProgramAnnotationAttribute(attr));

            foreach (var attribute in classLevelAttributes)
            {
                _writer.WriteAddClassAttribute(attribute);
            }
        }
    }
}
