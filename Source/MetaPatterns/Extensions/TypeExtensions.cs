using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns.Extensions
{
    public static class TypeExtensions
    {
        public static Type[] OrEmptyTypes(this Type[] types)
        {
            return (types ?? EmptyTypes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string FriendlyName(this TypeInfo type, string punctuation = null)
        {
            if (type.IsGenericType || type.IsNested)
            {
                var nameBuilder = new StringBuilder();
                AppendFriendlyName(type, nameBuilder, punctuation ?? FriendlyNamePunctuation);

                return nameBuilder.ToString();
            }
            else
            {
                return type.Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetClosedDeclaringType(this TypeInfo nestedType, out Type[] nestedTypeArguments)
        {
            var declaringType = nestedType.DeclaringType?.GetTypeInfo();

            if (declaringType != null &&
                declaringType.IsGenericType &&
                declaringType.IsGenericTypeDefinition &&
                !nestedType.IsGenericTypeDefinition)
            {
                var declaringTypeArgumentCount = declaringType.GenericTypeParameters.Length;
                var allTypeArguments = nestedType.GenericTypeArguments;

                var declaringTypeArguments = allTypeArguments.Take(declaringTypeArgumentCount).ToArray();
                nestedTypeArguments = allTypeArguments.Skip(declaringTypeArgumentCount).ToArray();

                return nestedType.DeclaringType.MakeGenericType(declaringTypeArguments);
            }

            nestedTypeArguments = null;
            return nestedType.DeclaringType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type[] EmptyTypes { get; } = new Type[0];
        public static string FriendlyNamePunctuation { get; } = ".`<,>";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void AppendFriendlyName(TypeInfo type, StringBuilder output, string punctuation)
        {
            Type[] nestedTypeArguments = null;

            if (type.IsNested && type.DeclaringType != null)
            {
                AppendFriendlyName(type.GetClosedDeclaringType(out nestedTypeArguments).GetTypeInfo(), output, punctuation);
                output.Append(punctuation[0]);
            }

            if (type.IsGenericType && (nestedTypeArguments == null || nestedTypeArguments.Length > 0))
            {
                var backquoteIndex = type.Name.IndexOf('`');

                output.Append(backquoteIndex > 0 ? type.Name.Substring(0, backquoteIndex) : type.Name);
                output.Append(punctuation[2]);

                var typeArguments = nestedTypeArguments ?? type.GenericTypeArguments;

                for (int i = 0; i < typeArguments.Length; i++)
                {
                    AppendFriendlyName(typeArguments[i].GetTypeInfo(), output, punctuation);

                    if (i < typeArguments.Length - 1)
                    {
                        output.Append(punctuation[3]);
                    }
                }

                output.Append(punctuation[4]);
            }
            else
            {
                output.Append(type.Name);
            }
        }
    }
}
