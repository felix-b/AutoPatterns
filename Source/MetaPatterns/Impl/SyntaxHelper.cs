using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MetaPatterns.Impl
{
    internal static class SyntaxHelper
    {
        public static ArgumentListSyntax CopyParametersToArguments(ParameterListSyntax parameters)
        {
            return SyntaxFactory.ArgumentList(SeparatedList<ArgumentSyntax>(
                parameters.Parameters.Select(
                    p => Argument(IdentifierName(p.Identifier.ValueText))
                )
            ));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeSyntax GetTypeSyntax(Type type)
        {
            return GetTypeSyntax(GetTypeFullNameParts(type));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeSyntax GetTypeSyntax(string[] parts)
        {
            return GetTypeSyntax(parts, parts.Length);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string[] GetTypeFullNameParts(Type type)
        {
            var parts = type.FullName.Split('.');

            if (type.IsNested)
            {
                var nestedParts = parts[parts.Length - 1].Split('+');
                var newParts = new string[parts.Length - 1 + nestedParts.Length];

                for (int i = 0 ; i < parts.Length - 1 ; i++)
                {
                    newParts[i] = parts[i];
                }

                for (int i = 0 ; i < nestedParts.Length ; i++)
                {
                    newParts[parts.Length - 1 + i] = nestedParts[i];
                }

                parts = newParts;
            }

            return parts;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static NameSyntax GetTypeSyntax(string[] parts, int takeCount)
        {
            if (takeCount == 1)
            {
                return IdentifierName(parts[0]);
            }
            else if (takeCount == 2)
            {
                return QualifiedName(IdentifierName(parts[0]), IdentifierName(parts[1]));
            }
            else
            {
                return QualifiedName(GetTypeSyntax(parts, takeCount - 1), IdentifierName(parts[takeCount - 1]));
            }
        }
    }
}
