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
using static AutoPatterns.MetaProgram.Annotation;

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
        private readonly Dictionary<ISymbol, MetaProgram.Annotation.MetaMemberAttribute> _metaMemberAttributes;
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
            _metaMemberAttributes = new Dictionary<ISymbol, MetaProgram.Annotation.MetaMemberAttribute>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildApplyMethod()
        {
            ScanTemplateMetaMembers();

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
            ImplementClassAttributes();
            ImplementBaseTypes();
            ImplementRepeatOnceMembers();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementClassAttributes()
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementBaseTypes()
        {
            if (_templateClassSyntax.BaseList != null)
            {
                foreach (var baseSyntax in _templateClassSyntax.BaseList.Types)
                {
                    _writer.WriteAddBaseType(baseSyntax.Type);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ScanTemplateMetaMembers()
        {
            var metaMembers = _templateClassSymbol.GetMembers()
                .Where(m => m.HasAttribute(_writer.MetaMemberAttributeTypeSymbol))
                .ToArray();

            foreach (var member in metaMembers)
            {
                var metaAttribute = member.GetAttributes().First(attr => attr.AttributeClass.Equals(_writer.MetaMemberAttributeTypeSymbol));
                var repeatValue = metaAttribute.NamedArguments.AsEnumerable().FirstOrDefault(arg => arg.Key == nameof(MetaMemberAttribute.Repeat));
                var selectValue = metaAttribute.NamedArguments.AsEnumerable().FirstOrDefault(arg => arg.Key == nameof(MetaMemberAttribute.Select));

                var attributeInstance = new MetaMemberAttribute();

                if (repeatValue.Key != null)
                {
                    attributeInstance.Repeat = (RepeatOption)repeatValue.Value.Value;
                }

                if (selectValue.Key != null)
                {
                    attributeInstance.Select = (SelectOptions)selectValue.Value.Value;
                }

                _metaMemberAttributes.Add(member, attributeInstance);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementRepeatOnceMembers()
        {
            foreach (var memberAttributePair in _metaMemberAttributes.Where(kvp => kvp.Value.Repeat == RepeatOption.Once))
            {
                var member = memberAttributePair.Key;
                var attribute = memberAttributePair.Value;

                switch (member.Kind)
                {
                    case SymbolKind.Method:
                        var syntaxRef = ((IMethodSymbol)member).DeclaringSyntaxReferences.First();
                        var syntax = ((MethodDeclarationSyntax)syntaxRef.GetSyntax());
                        _writer.WriteAddMethod(syntax);
                        break;
                }
            }
        }
    }
}
