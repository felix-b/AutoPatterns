using System;
using System.Collections.Generic;
using System.Linq;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoPatterns
{
    public class MetaCompilerContext
    {
        internal MetaCompilerContext(AutoPatternCompiler compiler, TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            this.Input = new InputContext(
                typeKey, 
                baseType, 
                primaryInterfaces.OrEmptyTypes(),
                secondaryInterfaces.OrEmptyTypes());

            this.Output = new OutputContext(compiler, typeKey);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InputContext Input { get; private set; }
        public OutputContext Output { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class InputContext
        {
            public InputContext(TypeKey typeKey, Type baseType, IReadOnlyList<Type> primaryInterfaces, IReadOnlyList<Type> secondaryInterfaces)
            {
                TypeKey = typeKey;
                BaseType = baseType;
                PrimaryInterfaces = primaryInterfaces;
                SecondaryInterfaces = secondaryInterfaces;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeKey TypeKey { get; private set; }
            public Type BaseType { get; private set; }
            public IReadOnlyList<Type> PrimaryInterfaces { get; private set; }
            public IReadOnlyList<Type> SecondaryInterfaces { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OutputContext
        {
            internal OutputContext(AutoPatternCompiler compiler, TypeKey typeKey)
            {
                this.ClassNamespace = compiler.NamespaceName;
                this.ClassName = typeKey.ToString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public string ClassNamespace { get; private set; }
            public string ClassName { get; private set; }
            public List<BaseTypeSyntax> BaseTypes { get; } = new List<BaseTypeSyntax>();
            public List<FieldDeclarationSyntax> Fields { get; } = new List<FieldDeclarationSyntax>();
            public List<ConstructorDeclarationSyntax> Constructors { get; } = new List<ConstructorDeclarationSyntax>();
            public List<MethodDeclarationSyntax> Methods { get; } = new List<MethodDeclarationSyntax>();
            public List<PropertyDeclarationSyntax> Properties { get; } = new List<PropertyDeclarationSyntax>();
            public List<IndexerDeclarationSyntax> Indexers { get; } = new List<IndexerDeclarationSyntax>();
            public List<EventDeclarationSyntax> Events { get; } = new List<EventDeclarationSyntax>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal MemberDeclarationSyntax[] GetAllMembers()
            {
                return 
                    Fields.Cast<MemberDeclarationSyntax>()
                    .Concat(Constructors.Cast<MemberDeclarationSyntax>()
                    .Concat(Methods.Cast<MemberDeclarationSyntax>()
                    .Concat(Properties.Cast<MemberDeclarationSyntax>()
                    .Concat(Indexers.Cast<MemberDeclarationSyntax>()
                    .Concat(Events.Cast<MemberDeclarationSyntax>())))))
                    .ToArray();
            }
        }
    }
}
