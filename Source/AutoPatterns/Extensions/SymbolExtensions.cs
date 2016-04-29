using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoPatterns.Extensions
{
    public static class SymbolExtensions
    {
        public static string FullName(this ISymbol symbol)
        {
            var namespaceName = symbol.ContainingNamespace?.FullName();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                return namespaceName + "." + symbol.Name;
            }
            else
            {
                return symbol.Name;
            }
        }
        public static NameSyntax FullNameSyntax(this ISymbol symbol)
        {
            return SyntaxFactory.ParseName(FullName(symbol));
        }
    }
}
