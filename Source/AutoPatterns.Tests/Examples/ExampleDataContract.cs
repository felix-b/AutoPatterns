using System.Reflection;
using System.Runtime.Serialization;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    [DataContract(Name = MetaProgram.Constant.String1, Namespace = MetaProgram.Constant.String2)]
    public partial class ExampleDataContract
    {
        [MetaProgram.Annotation.MetaMember(Implement = ImplementOptions.Attributes)]
        [DataMember]
        public object AProperty { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public partial class ExampleDataContract : IPatternTemplate
    {
        void IPatternTemplate.Apply(PatternWriterContext context)
        {
            ExampleDataContract__Apply(context);

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

        private void ExampleDataContract__Apply(PatternWriterContext context)
        {
            context.Output.ClassWriter.AddClassAttribute(typeof(System.Runtime.Serialization.DataContractAttribute));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool AProperty__Match(PatternWriterContext context, PropertyInfo declaration)
        {
            return (declaration.CanRead && declaration.CanWrite && declaration.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AProperty__Apply(PatternWriterContext context, PropertyInfo declaration)
        {
            var property = context.Output.ClassWriter.TryGetMember<ClassWriter.PropertyMember>(declaration);

            if (property != null)
            {
                context.Library.EnsureMetadataReference(typeof(System.Runtime.Serialization.DataMemberAttribute));
                property.Syntax = property.Syntax.WithAttributeLists(
                    property.Syntax.AttributeLists.Add(AttributeList(SeparatedList<AttributeSyntax>(new AttributeSyntax[] {
                        Attribute(SyntaxHelper.GetTypeSyntax(typeof(System.Runtime.Serialization.DataMemberAttribute)))    
                    }))));
            }
        }
    }
}
