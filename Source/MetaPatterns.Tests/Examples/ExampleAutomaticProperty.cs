using System.Reflection;
using MetaPatterns.Abstractions;
using MetaPatterns.Impl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MetaPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    public partial class ExampleAutomaticProperty
    {
        [MetaProgram.Annotation.MetaMember]
        public object AProperty { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public partial class ExampleAutomaticProperty : IMetaPatternTemplate
    {
        void IMetaPatternTemplate.Apply(MetaPatternCompilerContext context)
        {
            foreach (var interfaceType in context.Input.PrimaryInterfaces)
            {
                foreach (var property in interfaceType.GetTypeInfo().DeclaredProperties)
                {
                    if (Match__AProperty(context, property))
                    {
                        Apply__AProperty(context, property);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool Template__ShouldApply(MetaPatternCompilerContext context)
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Template__BeginApply(MetaPatternCompilerContext context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Template__EndApply(MetaPatternCompilerContext context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool Match__AProperty(MetaPatternCompilerContext context, PropertyInfo declaration)
        {
            return (declaration.CanRead && declaration.CanWrite && declaration.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Apply__AProperty(MetaPatternCompilerContext context, PropertyInfo declaration)
        {
            var syntax = PropertyDeclaration(SyntaxHelper.GetTypeSyntax(declaration.PropertyType), Identifier(declaration.Name))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(AccessorList(List<AccessorDeclarationSyntax>(
                    new AccessorDeclarationSyntax[] {
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    })
                 ));

            context.Output.Properties.Add(syntax);
        }
    }
}
