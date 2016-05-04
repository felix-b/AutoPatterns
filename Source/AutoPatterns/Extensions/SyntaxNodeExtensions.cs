using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoPatterns.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static bool HasAttributeSyntax(
            this ClassDeclarationSyntax classSyntax,
            ITypeSymbol attributeTypeSymbol,
            SemanticModel semanticModel)
        {
            var result = classSyntax.AttributeLists.Any(list => list.Attributes.Any(
                attr => {
                    var attrSymbolInfo = semanticModel.GetSymbolInfo(attr);
                    var attrTypeSymbol = attrSymbolInfo.Symbol.ContainingType;
                    var found = (attrTypeSymbol == attributeTypeSymbol);
                    return found;
                }));

            return result;
        }
    }
}
