using System.Reflection;
using System.Runtime.Serialization;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    [DataContract(Name = MetaProgram.Constant.String1, Namespace = MetaProgram.Constant.String2)]
    public partial class ExampleDataContract
    {
        [MetaProgram.Annotation.MetaMember]
        [DataMember]
        public object AProperty { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public partial class ExampleDataContract : IPatternTemplate
    {
        void IPatternTemplate.Apply(PatternWriterContext context)
        {
            throw new System.NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool AProperty__Match(PatternWriterContext context, PropertyInfo declaration)
        {
            return (declaration.CanRead && declaration.CanWrite && declaration.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AProperty__Apply(PatternWriterContext context, PropertyInfo declaration, PropertyDeclarationSyntax syntax)
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
