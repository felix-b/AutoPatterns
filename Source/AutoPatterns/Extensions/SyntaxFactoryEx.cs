using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Extensions
{
    public static class SyntaxFactoryEx
    {
        public static ArrayCreationExpressionSyntax ArrayOfObject(params ExpressionSyntax[] valueSyntaxes)
        {
            ExpressionSyntax rankSyntax = (
                valueSyntaxes?.Length > 0 
                ? (ExpressionSyntax)OmittedArraySizeExpression() 
                : (ExpressionSyntax)LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)));

            var arraySyntax = ArrayCreationExpression(
                ArrayType(
                    PredefinedType(
                        Token(SyntaxKind.ObjectKeyword)
                    )
                )
                .WithRankSpecifiers(
                    SingletonList<ArrayRankSpecifierSyntax>(
                        ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(rankSyntax)
                        )
                    )
                )
            );

            if (valueSyntaxes?.Length > 0)
            {
                arraySyntax = arraySyntax.WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList<ExpressionSyntax>(valueSyntaxes)
                    )
                );
            }

            return arraySyntax;
        }
    }
}
