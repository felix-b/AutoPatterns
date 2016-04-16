using System.Reflection;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    public partial class ExampleAutomaticProperty
    {
        [MetaProgram.Annotation.MetaMember, MetaProgram.Annotation.RepeatWith(nameof(AProperty))]
        private object m_AProperty;

        [MetaProgram.Annotation.MetaMember]
        public object AProperty
        {
            get { return m_AProperty; }
            set { m_AProperty = value; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public partial class ExampleAutomaticProperty : IPatternTemplate
    {
        void IPatternTemplate.Apply(PatternWriterContext context)
        {
            foreach (var interfaceType in context.Input.PrimaryInterfaces)
            {
                foreach (var property in interfaceType.GetTypeInfo().DeclaredProperties)
                {
                    if (AProperty__Match(context, property))
                    {
                        AProperty__Apply(context, property);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool AProperty__Match(PatternWriterContext context, PropertyInfo declaration)
        {
            return (declaration.CanRead && declaration.CanWrite && declaration.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AProperty__Apply(PatternWriterContext context, PropertyInfo declaration)
        {
            var entry = context.Output.ClassWriter.AddPublicProperty(declaration.Name, declaration.PropertyType, declaration);
                
            entry.Syntax = entry.Syntax
                .WithAccessorList(AccessorList(List<AccessorDeclarationSyntax>(
                    new[] {
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    })
                 ));
        }
    }
}
