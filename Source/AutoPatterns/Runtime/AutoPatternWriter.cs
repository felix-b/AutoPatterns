using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Runtime
{
    public sealed class AutoPatternWriter
    {
        public const string FactoryMethodNamePrefix = "FactoryMethod__";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly AutoPatternLibrary _library;
        private readonly string _namespaceName;
        private readonly Func<AutoPatternWriterContext, IAutoPatternTemplate[]> _onBuildPipeline;
        private readonly Func<TypeKey, string> _onGetClassName;
        private readonly ConcurrentDictionary<TypeKey, MemberDeclarationSyntax> _writtenMembers;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutoPatternWriter(
            AutoPatternLibrary library, 
            string namespaceName,
            Func<AutoPatternWriterContext, IAutoPatternTemplate[]> onBuildPipeline,
            Func<TypeKey, string> onGetClassName = null)
        {
            _library = library;
            _namespaceName = namespaceName;
            _onBuildPipeline = onBuildPipeline;
            _onGetClassName = onGetClassName ?? AutoPatternLibrary.GetDefaultClassName;
            _writtenMembers = new ConcurrentDictionary<TypeKey, MemberDeclarationSyntax>();

            library.AddWriter(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureWritten(TypeKey typeKey, Type baseType = null, Type primaryInterface = null)
        {
            EnsureWritten(
                typeKey, 
                baseType, 
                primaryInterfaces: primaryInterface != null ? new[] { primaryInterface } : null, 
                secondaryInterfaces: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureWritten(TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            _writtenMembers.GetOrAdd(
                typeKey, 
                k => WriteClass(typeKey, baseType, primaryInterfaces, secondaryInterfaces));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetClassName(TypeKey typeKey)
        {
            return _onGetClassName(typeKey);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MemberDeclarationSyntax[] TakeWrittenMembers()
        {
            var members = _writtenMembers.Values.ToArray();
            _writtenMembers.Clear();
            return members;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NamespaceName => _namespaceName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax WriteClass(
            TypeKey typeKey, 
            Type baseType, 
            Type[] primaryInterfaces, 
            Type[] secondaryInterfaces)
        {
            var context = new AutoPatternWriterContext(this, typeKey, baseType, primaryInterfaces, secondaryInterfaces);
            var pipeline = _onBuildPipeline(context);

            WriteBaseTypes(context);

            for (int i = 0; i < pipeline.Length; i++)
            {
                pipeline[i].Apply(context);
            }

            var syntax = GetCompleteClassSyntax(context);
            return syntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteBaseTypes(AutoPatternWriterContext context)
        {
            _library.EnsureMetadataReference(typeof(object));

            if (context.Input.BaseType != null)
            {
                AddBaseType(context.Input.BaseType, context);
            }

            foreach (var interfaceType in context.Input.PrimaryInterfaces)
            {
                AddBaseType(interfaceType, context);
            }

            foreach (var interfaceType in context.Input.SecondaryInterfaces)
            {
                AddBaseType(interfaceType, context);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddBaseType(Type type, AutoPatternWriterContext context)
        {
            _library.EnsureMetadataReference(type);
            context.Output.BaseTypes.Add(SimpleBaseType(SyntaxHelper.GetTypeSyntax(type)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax GetCompleteClassSyntax(AutoPatternWriterContext context)
        {
            WriteFactoryMethods(context);

            return 
                NamespaceDeclaration(IdentifierName(context.Output.ClassNamespace))
                .WithUsings(
                    SingletonList<UsingDirectiveSyntax>(UsingDirective(IdentifierName("System")))
                )
                .WithMembers(
                    List<MemberDeclarationSyntax>(
                        new MemberDeclarationSyntax[] {
                            ClassDeclaration(context.Output.ClassName)
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(context.Output.BaseTypes)))
                                .WithMembers(List<MemberDeclarationSyntax>(
                                    context.Output.GetAllMembers()
                                ))
                        }
                    )
                );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteFactoryMethods(AutoPatternWriterContext context)
        {
            var constructorList = context.Output.Constructors;

            if (constructorList.Count > 0)
            {
                for (int index = 0 ; index < constructorList.Count; index++)
                {
                    WriteFactoryMethod(context, constructorList[index], index);
                }
            }
            else
            {
                WriteDefaultFactoryMethod(context);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteFactoryMethod(AutoPatternWriterContext context, ConstructorDeclarationSyntax constructor, int index)
        {
            var factoryMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)), Identifier($"FactoryMethod__{index}"))
                .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                .WithParameterList(constructor.ParameterList)
                .WithBody(Block(SingletonList<StatementSyntax>(
                    ReturnStatement(
                        ObjectCreationExpression(IdentifierName(context.Output.ClassName))
                            .WithArgumentList(SyntaxHelper.CopyParametersToArguments(constructor.ParameterList)))
                 )));

            context.Output.Methods.Add(factoryMethod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteDefaultFactoryMethod(AutoPatternWriterContext context)
        {
            var factoryMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)), Identifier("FactoryMethod__0"))
                .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                .WithBody(Block(SingletonList<StatementSyntax>(
                    ReturnStatement(ObjectCreationExpression(IdentifierName(context.Output.ClassName)).WithArgumentList(ArgumentList())))
                ));

            context.Output.Methods.Add(factoryMethod);
        }
    }
}
