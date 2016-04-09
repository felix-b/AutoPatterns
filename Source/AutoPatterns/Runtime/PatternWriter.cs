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
    public sealed class PatternWriter
    {
        public const string FactoryMethodNamePrefix = "FactoryMethod__";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly AutoPattern _ownerPattern;
        private readonly ConcurrentDictionary<TypeKey, MemberDeclarationSyntax> _writtenMembers;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternWriter(AutoPattern ownerPattern)
        {
            _ownerPattern = ownerPattern;
            _writtenMembers = new ConcurrentDictionary<TypeKey, MemberDeclarationSyntax>();
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

        public AutoPattern OwnerPattern => _ownerPattern;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MemberDeclarationSyntax[] TakeWrittenMembers()
        {
            var members = _writtenMembers.Values.ToArray();
            _writtenMembers.Clear();
            return members;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax WriteClass(
            TypeKey typeKey, 
            Type baseType, 
            Type[] primaryInterfaces, 
            Type[] secondaryInterfaces)
        {
            var context = new PatternWriterContext(this, typeKey, baseType, primaryInterfaces, secondaryInterfaces);
            var pipeline = _ownerPattern.InternalBuildPipeline(context);

            WriteBaseTypes(context);

            for (int i = 0; i < pipeline.Length; i++)
            {
                pipeline[i].Apply(context);
            }

            var syntax = GetCompleteClassSyntax(context);
            return syntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteBaseTypes(PatternWriterContext context)
        {
            _ownerPattern.Library.EnsureMetadataReference(typeof(object));

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

        private void AddBaseType(Type type, PatternWriterContext context)
        {
            _ownerPattern.Library.EnsureMetadataReference(type);
            context.Output.BaseTypes.Add(SimpleBaseType(SyntaxHelper.GetTypeSyntax(type)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax GetCompleteClassSyntax(PatternWriterContext context)
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

        private void WriteFactoryMethods(PatternWriterContext context)
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

        private void WriteFactoryMethod(PatternWriterContext context, ConstructorDeclarationSyntax constructor, int index)
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

        private void WriteDefaultFactoryMethod(PatternWriterContext context)
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
