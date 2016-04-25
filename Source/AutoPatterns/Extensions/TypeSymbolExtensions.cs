using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace AutoPatterns.Extensions
{
    public static class TypeSymbolExtensions
    {
        public static bool IsSystemVoid(this ITypeSymbol type)
        {
            return (type != null && type.SpecialType == SpecialType.System_Void);
        }
    }
}
