using System.Reflection;
using AutoPatterns.DesignTime;
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
        [MetaProgram.Annotation.MetaMember, MetaProgram.Annotation.IncludeWith(nameof(AProperty))]
        private MetaProgram.TypeRef.TProperty _aProperty;

        [MetaProgram.Annotation.MetaMember]
        public MetaProgram.TypeRef.TProperty AProperty
        {
            get { return _aProperty; }
            set { _aProperty = value; }
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
            var backingField = context.Output.ClassWriter.AddPrivateField("_" + declaration.Name.ToCamelCase(), declaration.PropertyType);
            var property = context.Output.ClassWriter.AddPublicProperty(declaration.Name, declaration.PropertyType, declaration);

            if (property.Getter != null)
            {
                property.Getter = property.Getter.WithBody(Block(
                    SingletonList<StatementSyntax>(
                        ReturnStatement(
                            IdentifierName(backingField.Name)
                        )
                    )
                ));
            }

            if (property.Setter != null)
            {
                property.Setter = property.Setter.WithBody(Block(
                    SingletonList<StatementSyntax>(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(backingField.Name),
                                IdentifierName("value")
                            )
                        )
                    )
                ));
            }
        }
    }
}
