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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static AutoPatterns.Extensions.SyntaxFactoryEx;

namespace AutoPatterns.DesignTime
{
    public class ClassWriterClientWriter
    {
        private readonly SemanticModel _semanticModel;
        private readonly List<StatementSyntax> _statements;
        private readonly ITypeSymbol _metaProgramAnnotationTypeSymbol;
        private readonly INamedTypeSymbol _classTemplateAttributeTypeSymbol;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ClassWriterClientWriter(SemanticModel semanticModel, List<StatementSyntax> statements)
        {
            _semanticModel = semanticModel;
            _statements = statements;
            _metaProgramAnnotationTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(_s_metaProgramAnnotationTypeFullName);
            _classTemplateAttributeTypeSymbol = _semanticModel.Compilation.GetTypeByMetadataName(_s_classTemplateAttributeFullName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteAddClassAttribute(AttributeSyntax attribute)
        {
            var attributeTypeSymbol = _semanticModel.GetSymbolInfo(attribute).Symbol.ContainingType;

            var positionalArguments = attribute.ArgumentList?.Arguments
                .Where(arg => arg.NameEquals == null)
                .Select(arg => arg.Expression)
                .ToArray();

            var namedArguments = attribute.ArgumentList?.Arguments
                .Where(arg => arg.NameEquals != null)
                .SelectMany(arg => new[] {
                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(arg.NameEquals.Name.Identifier.ValueText)),
                    arg.Expression
                })
                .ToArray();

            var statement = ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        _s_writerAccessExpression,
                        IdentifierName(nameof(ClassWriter.AddClassAttribute))
                    )
                )
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new ArgumentSyntax[] {
                                Argument(TypeOfExpression(ParseTypeName(attributeTypeSymbol.ToDisplayString()))),
                                Argument(ArrayOfObject(positionalArguments)),
                                Argument(ArrayOfObject(namedArguments))
                            }
                        )
                    )
                )
            );

            _statements.Add(statement);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsMetaProgramAnnotationAttribute(AttributeSyntax attribute)
        {
            var actualSymbolInfo = _semanticModel.GetSymbolInfo(attribute);
            var containingTypeSymbol = actualSymbolInfo.Symbol.ContainingType?.ContainingType;
            var result = (containingTypeSymbol != null && containingTypeSymbol.Equals(_metaProgramAnnotationTypeSymbol));

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAttributeOfType(AttributeSyntax attribute, ITypeSymbol expectedTypeSymbol)
        {
            var actualSymbolInfo = _semanticModel.GetSymbolInfo(attribute);
            var actualTypeSymbol = actualSymbolInfo.Symbol.ContainingType;
            var result = actualTypeSymbol.Equals(expectedTypeSymbol);

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsClassTemplateAttribute(AttributeSyntax attribute)
        {
            return IsAttributeOfType(attribute, _classTemplateAttributeTypeSymbol);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_metaProgramAnnotationTypeFullName = typeof(MetaProgram.Annotation).FullName;
        private static readonly string _s_classTemplateAttributeFullName = typeof(MetaProgram.Annotation.ClassTemplateAttribute).FullName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly MemberAccessExpressionSyntax _s_writerAccessExpression =
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("context"),
                    IdentifierName(nameof(PatternWriterContext.Output))
                ),
                IdentifierName(nameof(PatternWriterContext.OutputContext.ClassWriter))
            );
    }
}
