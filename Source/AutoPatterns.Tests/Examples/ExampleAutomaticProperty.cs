using System.Reflection;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    public partial class ExampleAutomaticProperty
    {
        [MetaProgram.Annotation.MetaMember]
        public object AProperty { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public partial class ExampleAutomaticProperty : IAutoPatternTemplate
    {
        void IAutoPatternTemplate.Apply(MetaCompilerContext context)
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

        private bool Template__ShouldApply(MetaCompilerContext context)
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Template__BeginApply(MetaCompilerContext context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Template__EndApply(MetaCompilerContext context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool Match__AProperty(MetaCompilerContext context, PropertyInfo declaration)
        {
            return (declaration.CanRead && declaration.CanWrite && declaration.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Apply__AProperty(MetaCompilerContext context, PropertyInfo declaration)
        {
            var syntax = SyntaxFactory.PropertyDeclaration(SyntaxHelper.GetTypeSyntax(declaration.PropertyType), SyntaxFactory.Identifier(declaration.Name))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(
                    new AccessorDeclarationSyntax[] {
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    })
                 ));

            context.Output.Properties.Add(syntax);
        }
    }
}
