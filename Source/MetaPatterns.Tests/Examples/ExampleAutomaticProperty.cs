using System.Reflection;
using MetaPatterns.Abstractions;
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

    public partial class ExampleAutomaticProperty : IMetaPatternTemplate
    {
        void IMetaPatternTemplate.Compile(MetaPatternCompilerContext context)
        {
            foreach (var interfaceType in context.Input.PrimaryInterfaces)
            {
                foreach (var property in interfaceType.GetTypeInfo().DeclaredProperties)
                {
                    if (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
                    {
                    }
                }
            }
            context.Output.Properties.Add(PropertyDeclaration(IdentifierName("Int32"), Identifier("IntValue"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(AccessorList(List<AccessorDeclarationSyntax>(
                    new AccessorDeclarationSyntax[] {
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    })
                 ))
             );
        }
    }
}
